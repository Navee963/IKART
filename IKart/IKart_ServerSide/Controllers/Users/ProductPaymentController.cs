// IKart_ServerSide/Controllers/Users/ProductPaymentController.cs
using IKart_ServerSide.Models;
using IKart_Shared.DTOs.Payment;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Linq;
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

                return Ok(new { Message = "Payment & Order successful!", PaymentId = payment.PaymentId, OrderId = order.Order_Id });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Payment verification failed: " + ex.Message));
            }
        }

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

            // EMI logic: create Monthly_EMI_Calc & Installment_Payments if tenure > 1
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

            return Ok(new { Message = "Payment completed!", PaymentId = payment.PaymentId, OrderId = order.Order_Id });
        }
    }
}