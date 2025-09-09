using IKart_ServerSide.Models;
using IKart_Shared.DTOs.Payment;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;

namespace IKart_ServerSide.Controllers.Users
{
    [RoutePrefix("api/payments")]
    public class ProductPaymentController : ApiController
    {
        private IKartEntities db = new IKartEntities();
        private string razorpayKey = "rzp_test_REOSbBMiZHhMMR";
        private string razorpaySecret = "uTudflYReN7PqR4ZtSitanfZ";
        private decimal platformFee = 100m;
        private decimal processingFee = 50m;

        // ------------------------
        // 1. Get Payment Options
        // ------------------------
        [HttpGet]
        [Route("options/{userId}/{productId}")]
        public IHttpActionResult GetPaymentOptions(int userId, int productId)
        {
            var user = db.Users.Find(userId);
            var product = db.Products.Find(productId);
            if (user == null || product == null) return NotFound();

            var cards = db.EMI_Card.Where(c => c.UserId == userId && c.IsActive == true).ToList();
            var cardList = cards.Select(c => new { c.EmiCardId, c.CardType, c.Balance }).ToList();

            return Ok(new
            {
                Cards = cardList,
                ProductCost = product.Cost,
                PlatformFee = platformFee,
                ProcessingFee = processingFee
            });
        }

        // ------------------------
        // 2. Create Razorpay Order
        // ------------------------
        [HttpPost]
        [Route("razorpay-order")]
        public IHttpActionResult CreateRazorpayOrder(OrderRequestDto request)
        {
            var product = db.Products.Find(request.ProductId);
            if (product == null) return NotFound();

            var client = new RazorpayClient(razorpayKey, razorpaySecret);
            decimal totalAmount = (decimal)product.Cost + platformFee + processingFee;

            Dictionary<string, object> options = new Dictionary<string, object>
            {
                { "amount", (int)(totalAmount * 100) },
                { "currency", "INR" },
                { "payment_capture", 1 }
            };

            var order = client.Order.Create(options);

            return Ok(new
            {
                orderId = order["id"],
                amount = order["amount"],
                currency = order["currency"],
                productName = product.ProductName
            });
        }

        // ------------------------
        // 3. Verify Razorpay Payment
        // ------------------------
        [HttpPost]
        [Route("razorpay-verify")]
        public IHttpActionResult VerifyRazorpayPayment(VerifyPaymentDto dto)
        {
            try
            {
                var client = new RazorpayClient(razorpayKey, razorpaySecret);
                Dictionary<string, string> attributes = new Dictionary<string, string>
                {
                    { "razorpay_order_id", dto.RazorpayOrderId },
                    { "razorpay_payment_id", dto.RazorpayPaymentId },
                    { "razorpay_signature", dto.RazorpaySignature }
                };

                Utils.verifyPaymentSignature(attributes);

                var product = db.Products.Find(dto.ProductId);
                var user = db.Users.Find(dto.UserId);
                if (product == null || user == null) return NotFound();

                decimal totalAmount = (decimal)product.Cost + platformFee + processingFee;

                var payment = new Models.Payment
                {
                    EmiCardId = null,
                    UserId = dto.UserId,
                    ProductId = dto.ProductId,
                    PaymentMethodId = db.Payment_Methods.FirstOrDefault(m => m.MethodName == "Razorpay")?.PaymentMethodId ?? 4,
                    ProcessingFee = processingFee,
                    TotalAmount = totalAmount,
                    RazorpayPaymentId = dto.RazorpayPaymentId,
                    PaymentDate = DateTime.Now,
                    Status = "Paid"
                };

                db.Payments.Add(payment);
                db.SaveChanges();

                var order = new Models.Order
                {
                    ProductId = dto.ProductId,
                    UserId = dto.UserId,
                    PaymentId = payment.PaymentId,
                    OrderDate = DateTime.Now,
                    DeliveryDate = DateTime.Now.AddDays(5),
                    Region = null,
                    OrderStatus = "Ordered"
                };
                db.Orders.Add(order);
                db.SaveChanges();

                // ✅ Reduce stock
                ReduceStock(dto.ProductId);

                return Ok(new { Message = "Payment & Order successful!", PaymentId = payment.PaymentId, OrderId = order.Order_Id });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Payment verification failed: " + ex.Message));
            }
        }

        // ------------------------
        // 4. Pay Using EMI Card
        // ------------------------
        [HttpPost]
        [Route("pay-card")]
        public IHttpActionResult PayUsingCard(CardPaymentDto dto)
        {
            var user = db.Users.Find(dto.UserId);
            var product = db.Products.Find(dto.ProductId);
            var card = db.EMI_Card.FirstOrDefault(c => c.UserId == dto.UserId && c.IsActive == true && c.EmiCardId == dto.EmiCardId);

            if (user == null || product == null || card == null)
                return NotFound();

            decimal totalAmount = (decimal)product.Cost + platformFee + processingFee;
            if (card.Balance < totalAmount)
                return BadRequest("Insufficient card balance.");

            card.Balance -= totalAmount;

            var payment = new Models.Payment
            {
                EmiCardId = card.EmiCardId,
                UserId = user.UserId,
                ProductId = product.ProductId,
                PaymentMethodId = db.Payment_Methods.FirstOrDefault(m => m.MethodName == "Card")?.PaymentMethodId ?? 5,
                ProcessingFee = processingFee,
                TotalAmount = totalAmount,
                PaymentDate = DateTime.Now,
                Status = dto.TenureMonths > 1 ? "Pending" : "Paid"
            };
            db.Payments.Add(payment);
            db.SaveChanges();

            var order = new Models.Order
            {
                ProductId = product.ProductId,
                UserId = user.UserId,
                PaymentId = payment.PaymentId,
                OrderDate = DateTime.Now,
                DeliveryDate = DateTime.Now.AddDays(5),
                Region = null,
                OrderStatus = "Processing"
            };
            db.Orders.Add(order);
            db.SaveChanges();

            // ✅ Reduce stock
            ReduceStock(dto.ProductId);

            if (dto.TenureMonths > 1)
            {
                var emiCalc = new Monthly_EMI_Calc
                {
                    PaymentId = payment.PaymentId,
                    UserId = user.UserId,
                    TotalAmount = totalAmount,
                    EMIAmount = Math.Round(totalAmount / dto.TenureMonths, 2),
                    TenureMonths = dto.TenureMonths,
                    RemainingAmount = totalAmount
                };
                db.Monthly_EMI_Calc.Add(emiCalc);
                db.SaveChanges();

                for (int i = 1; i <= dto.TenureMonths; i++)
                {
                    db.Installment_Payments.Add(new Installment_Payments
                    {
                        EMI_Id = emiCalc.EMI_Id,
                        DueDate = DateTime.Now.AddMonths(i),
                        Amount = emiCalc.EMIAmount,
                        IsPaid = false
                    });
                }
                db.SaveChanges();
            }

            return Ok(new { Message = "Payment completed using card balance, Order Created!", PaymentId = payment.PaymentId, OrderId = order.Order_Id });
        }

        // ------------------------
        // 5. Pay Using Other Methods
        // ------------------------
        [HttpPost]
        [Route("pay-other")]
        public IHttpActionResult PayOther(PaymentDto dto)
        {
            var user = db.Users.Find(dto.UserId);
            var product = db.Products.Find(dto.ProductId);

            if (user == null || product == null)
                return NotFound();

            decimal totalAmount = (decimal)product.Cost + platformFee + processingFee;

            var payment = new Models.Payment
            {
                UserId = user.UserId,
                ProductId = product.ProductId,
                PaymentMethodId = db.Payment_Methods.FirstOrDefault(m => m.MethodName == dto.MethodName)?.PaymentMethodId ?? 6,
                ProcessingFee = processingFee,
                TotalAmount = totalAmount,
                PaymentDate = DateTime.Now,
                Status = "Paid"
            };

            db.Payments.Add(payment);
            db.SaveChanges();

            var order = new Models.Order
            {
                ProductId = product.ProductId,
                UserId = user.UserId,
                PaymentId = payment.PaymentId,
                OrderDate = DateTime.Now,
                DeliveryDate = DateTime.Now.AddDays(5),
                Region = null,
                OrderStatus = "Processing"
            };
            db.Orders.Add(order);
            db.SaveChanges();

            // ✅ Reduce stock
            ReduceStock(dto.ProductId);

            return Ok(new { Message = "Payment completed!", PaymentId = payment.PaymentId, OrderId = order.Order_Id });
        }

        // ------------------------
        // 6. Get All User Payments + Installments
        // ------------------------
        [HttpGet]
        [Route("user/{userId}")]
        public IHttpActionResult GetUserPayments(int userId)
        {
            var paymentsEntities = db.Payments.Where(p => p.UserId == userId).ToList();
            var result = new List<UserPaymentDto>();

            foreach (var p in paymentsEntities)
            {
                var productName = p.Product != null ? p.Product.ProductName : string.Empty;

                string cardType = null;
                if (p.EmiCardId != null)
                {
                    var card = db.EMI_Card.FirstOrDefault(c => c.EmiCardId == p.EmiCardId);
                    if (card != null) cardType = card.CardType;
                }

                var emiCalc = db.Monthly_EMI_Calc.FirstOrDefault(m => m.PaymentId == p.PaymentId);
                int tenureMonths = (int)(emiCalc != null ? emiCalc.TenureMonths : 0);
                decimal emiAmount = (decimal)(emiCalc != null ? emiCalc.EMIAmount : 0m);

                var instDtos = new List<InstallmentDto>();
                if (emiCalc != null)
                {
                    var instEntities = db.Installment_Payments.Where(i => i.EMI_Id == emiCalc.EMI_Id).ToList();
                    foreach (var inst in instEntities)
                    {
                        var installmentIdObj = GetPropertyValueSafe(inst, "InstallmentId") as int?;
                        var dueDateObj = GetPropertyValueSafe(inst, "DueDate") as DateTime?;
                        var amountObj = GetPropertyValueSafe(inst, "Amount") as decimal?;
                        var isPaidObj = GetPropertyValueSafe(inst, "IsPaid") as bool?;

                        instDtos.Add(new InstallmentDto
                        {
                            InstallmentId = installmentIdObj ?? 0,
                            DueDate = dueDateObj ?? DateTime.MinValue,
                            Amount = amountObj ?? 0m,
                            IsPaid = isPaidObj ?? false,
                            Penalty = null
                        });
                    }
                }

                var dto = new UserPaymentDto
                {
                    PaymentId = p.PaymentId,
                    ProductName = productName,
                    CardType = cardType,
                    TotalAmount = p.TotalAmount,
                    PaymentDate = (DateTime)p.PaymentDate,
                    Status = p.Status,
                    TenureMonths = tenureMonths,
                    EMIAmount = emiAmount,
                    Installments = instDtos
                };

                result.Add(dto);
            }

            return Ok(result);
        }

        // ------------------------
        // 7. Pay Single Installment
        // ------------------------
        [HttpPost]
        [Route("pay-installment/{installmentId}")]
        public IHttpActionResult PayInstallment(int installmentId)
        {
            var installment = db.Installment_Payments.FirstOrDefault(i => i.InstallmentId == installmentId);
            if (installment == null) return NotFound();

            object isPaidObj = GetPropertyValueSafe(installment, "IsPaid");
            bool alreadyPaid = isPaidObj != null && Convert.ToBoolean(isPaidObj);
            if (alreadyPaid) return BadRequest("Installment already paid.");

            var propIsPaid = installment.GetType().GetProperty("IsPaid");
            if (propIsPaid != null && propIsPaid.CanWrite)
            {
                propIsPaid.SetValue(installment, true);
            }
            db.SaveChanges();

            var emiIdObj = GetPropertyValueSafe(installment, "EMI_Id");
            if (emiIdObj != null)
            {
                int emiId = Convert.ToInt32(emiIdObj);
                var emiCalc = db.Monthly_EMI_Calc.FirstOrDefault(m => m.EMI_Id == emiId);
                if (emiCalc != null)
                {
                    var insts = db.Installment_Payments.Where(i => i.EMI_Id == emiCalc.EMI_Id).ToList();
                    bool allPaid = insts.All(i => Convert.ToBoolean(GetPropertyValueSafe(i, "IsPaid") ?? false));

                    if (allPaid)
                    {
                        var payment = db.Payments.FirstOrDefault(p => p.PaymentId == emiCalc.PaymentId);
                        if (payment != null)
                        {
                            payment.Status = "Paid";
                            db.SaveChanges();
                        }
                    }
                }
            }

            return Ok("Installment paid successfully.");
        }

        // ------------------------
        // Helper: Safe Property Getter
        // ------------------------
        private object GetPropertyValueSafe(object obj, string propName)
        {
            if (obj == null) return null;
            var pi = obj.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (pi == null) return null;
            return pi.GetValue(obj);
        }
        [HttpPost]
        [Route("installment-razorpay-order/{installmentId}")]
        public IHttpActionResult CreateInstallmentRazorpayOrder(int installmentId)
        {
            var installment = db.Installment_Payments.FirstOrDefault(i => i.InstallmentId == installmentId);
            if (installment == null) return NotFound();

            if ((bool)installment.IsPaid) return BadRequest("Installment already paid.");

            var client = new RazorpayClient(razorpayKey, razorpaySecret);

            Dictionary<string, object> options = new Dictionary<string, object>
    {
        { "amount", (int)(installment.Amount * 100) }, // Razorpay works in paise
        { "currency", "INR" },
        { "payment_capture", 1 }
    };

            var order = client.Order.Create(options);

            return Ok(new
            {
                orderId = order["id"],
                amount = order["amount"],
                currency = order["currency"],
                installmentId = installment.InstallmentId
            });
        }

        [HttpPost]
        [Route("installment-razorpay-verify")]
        public IHttpActionResult VerifyInstallmentPayment(VerifyPaymentDto dto)
        {
            try
            {
                var client = new RazorpayClient(razorpayKey, razorpaySecret);
                Dictionary<string, string> attributes = new Dictionary<string, string>
        {
            { "razorpay_order_id", dto.RazorpayOrderId },
            { "razorpay_payment_id", dto.RazorpayPaymentId },
            { "razorpay_signature", dto.RazorpaySignature }
        };

                Utils.verifyPaymentSignature(attributes);

                // Mark installment as paid
                var installment = db.Installment_Payments.FirstOrDefault(i => i.InstallmentId == dto.InstallmentId);
                if (installment == null) return NotFound();

                installment.IsPaid = true;
                db.SaveChanges();

                // Check if all installments are paid → update Payment status
                var emiCalc = db.Monthly_EMI_Calc.FirstOrDefault(m => m.EMI_Id == installment.EMI_Id);
                if (emiCalc != null)
                {
                    var insts = db.Installment_Payments.Where(i => i.EMI_Id == emiCalc.EMI_Id).ToList();
                    bool allPaid = insts.All(i => (bool)i.IsPaid);

                    if (allPaid)
                    {
                        var payment = db.Payments.FirstOrDefault(p => p.PaymentId == emiCalc.PaymentId);
                        if (payment != null)
                        {
                            payment.Status = "Paid";
                            db.SaveChanges();
                        }
                    }
                }

                return Ok(new { Message = "Installment paid successfully with Razorpay!" });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Payment verification failed: " + ex.Message));
            }
        }
        // ------------------------
        // Helper: Reduce Stock
        // ------------------------
        private void ReduceStock(int productId)
        {
            var product = db.Products.Find(productId);
            if (product == null) return;

            var stock = db.Stocks.FirstOrDefault(s => s.Stock_Id == product.Stock_Id);
            if (stock != null && stock.Available_Stocks > 0)
            {
                stock.Available_Stocks -= 1;
                db.SaveChanges();
            }
        }
    }
}
