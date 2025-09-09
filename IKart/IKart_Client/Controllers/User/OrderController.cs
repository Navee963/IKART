using IKart_Shared.DTOs;
using IKart_Shared.DTOs.Payment;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace IKart_Client.Controllers.User
{
    public class OrderController : Controller
    {
        string baseUrl = "https://localhost:44365/api/orders";

        [HttpPost]
        public async Task<ActionResult> BuyNow(int productId)
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            List<AddressDto> addresses = new List<AddressDto>();

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var response = await client.GetAsync($"https://localhost:44365/api/account/address/user/{userId}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        addresses = JsonConvert.DeserializeObject<List<AddressDto>>(json);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to load addresses.");
                        return View("Error");
                    }
                }
            }

            ViewBag.ProductId = productId;
            ViewBag.UserId = userId;

            return View("BuyNow", addresses);
        }

        [HttpPost]
        public async Task<ActionResult> ShowUPISummary(int productId, int addressId, int userId, string method)
        {
            ProductDto product = null;

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var res = await client.GetAsync($"https://localhost:44365/api/products/{productId}");
                    if (res.IsSuccessStatusCode)
                    {
                        var json = await res.Content.ReadAsStringAsync();
                        product = JsonConvert.DeserializeObject<ProductDto>(json);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to fetch product details.");
                        return View("Error");
                    }
                }
            }

            ViewBag.Product = product;
            ViewBag.AddressId = addressId;
            ViewBag.UserId = userId;
            ViewBag.Method = method;

            return View("ShowUPISummary");
        }

        public async Task<ActionResult> ChoosePayment(int productId, int addressId)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;

                using (HttpClient client = new HttpClient(handler))
                {
                    var response = await client.GetAsync($"https://localhost:44365/api/payments/options/{Session["UserId"]}/{productId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(json);
                        ViewBag.Cards = result.Cards;
                        ViewBag.ProductCost = result.ProductCost;
                        ViewBag.PlatformFee = result.PlatformFee;
                        ViewBag.ProcessingFee = result.ProcessingFee;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to load payment options.");
                        return View("Error");
                    }
                }
            }

            ViewBag.ProductId = productId;
            ViewBag.AddressId = addressId;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> PlaceOrder(int productId, int addressId, string method, int? emiCardId = null, int? tenureMonths = null)
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;

                using (HttpClient client = new HttpClient(handler))
                {
                    HttpResponseMessage res = null;

                    if (method == "Razorpay")
                    {
                        return RedirectToAction("InitiateRazorpay", new { productId, addressId });
                    }
                    else if (method == "Card" && emiCardId.HasValue && tenureMonths.HasValue)
                    {
                        var cardDto = new CardPaymentDto
                        {
                            UserId = userId,
                            ProductId = productId,
                            EmiCardId = emiCardId.Value,
                            TenureMonths = tenureMonths.Value
                        };
                        var json = JsonConvert.SerializeObject(cardDto);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        res = await client.PostAsync("https://localhost:44365/api/payments/pay-card", content);
                    }
                    else
                    {
                        var payDto = new PaymentDto
                        {
                            UserId = userId,
                            ProductId = productId,
                            MethodName = method
                        };
                        var json = JsonConvert.SerializeObject(payDto);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        res = await client.PostAsync("https://localhost:44365/api/payments/pay-other", content);
                    }

                    if (res != null && res.IsSuccessStatusCode)
                    {
                        ViewBag.Message = "Order Confirmed!";
                        return View("PaymentSuccess");
                    }

                    ModelState.AddModelError("", res != null ? await res.Content.ReadAsStringAsync() : "Payment Error");
                    return View("ChoosePayment");
                }
            }
        }

        [HttpGet]
        public async Task<ActionResult> InitiateRazorpay(int productId, int addressId)
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;

                using (HttpClient client = new HttpClient(handler))
                {
                    var orderRequest = new OrderRequestDto
                    {
                        UserId = userId,
                        ProductId = productId
                    };
                    var json = JsonConvert.SerializeObject(orderRequest);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = await client.PostAsync($"https://localhost:44365/api/payments/razorpay-order", content);

                    if (res.IsSuccessStatusCode)
                    {
                        var obj = JsonConvert.DeserializeObject<dynamic>(await res.Content.ReadAsStringAsync());
                        ViewBag.OrderId = obj.orderId;
                        ViewBag.Amount = obj.amount;
                        ViewBag.Currency = obj.currency;
                        ViewBag.ProductName = obj.productName;
                        ViewBag.ProductId = productId;
                        ViewBag.AddressId = addressId;
                        ViewBag.UserId = userId;
                        return View("RazorpayPayment");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to initiate Razorpay payment.");
                        return View("Error");
                    }
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> VerifyRazorpayPayment(VerifyPaymentDto dto)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;

                using (HttpClient client = new HttpClient(handler))
                {
                    var json = JsonConvert.SerializeObject(dto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = await client.PostAsync("https://localhost:44365/api/payments/razorpay-verify", content);

                    if (res.IsSuccessStatusCode)
                    {
                        ViewBag.Message = "Payment and Order successful!";
                        return View("PaymentSuccess");
                    }
                    else
                    {
                        var error = await res.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", error);
                        return View("Error");
                    }
                }
            }
        }

        public ActionResult PaymentSuccess()
        {
            ViewBag.Message = "Payment and Order successful!";
            return View();
        }
    }
}
