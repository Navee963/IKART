using IKart_Shared.DTOs;
using IKart_Shared.DTOs.Orders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IKart_Client.Controllers.User
{
    public class OrderController : Controller
    {
        string baseUrl = "https://localhost:44365/api/orders";

        // Razorpay key (test key, replace with your own)
        private readonly string RazorpayKey = "rzp_test_REOSbBMiZHhMMR";
        private readonly string RazorpaySecret = "uTudflYReN7PqR4ZtSitanfZ";

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

        public ActionResult ChoosePayment(int productId, int addressId)
        {
            ViewBag.ProductId = productId;
            ViewBag.AddressId = addressId;
            ViewBag.UserId = Convert.ToInt32(Session["UserId"]);
            return View();
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

        // RAZORPAY: Show Razorpay payment page
        [HttpPost]
        public async Task<ActionResult> RazorPayment(int productId, int addressId, int userId)
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

            // Amount in paise
            var amount = (int)((product.Cost + 100) * 100);

            // Call your backend to create Razorpay order and get orderId
            string razorpayOrderId = await CreateRazorpayOrder(amount);

            ViewBag.Product = product;
            ViewBag.AddressId = addressId;
            ViewBag.UserId = userId;
            ViewBag.Amount = amount;
            ViewBag.RazorpayKey = RazorpayKey;
            ViewBag.RazorpayOrderId = razorpayOrderId;

            return View("RazorpayPayment");
        }

        // Helper to create Razorpay order (server-side)
        private async Task<string> CreateRazorpayOrder(int amount)
        {
            // Minimal implementation, use Razorpay .NET SDK or plain HTTP POST
            using (var client = new HttpClient())
            {
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{RazorpayKey}:{RazorpaySecret}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                var payload = new
                {
                    amount = amount,
                    currency = "INR",
                    receipt = $"order_rcptid_{Guid.NewGuid()}",
                    payment_capture = 1
                };
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var res = await client.PostAsync("https://api.razorpay.com/v1/orders", content);
                var json = await res.Content.ReadAsStringAsync();
                dynamic obj = JsonConvert.DeserializeObject(json);
                return obj.id;
            }
        }

        // Razorpay payment callback (AJAX from client)
        [HttpPost]
        public async Task<ActionResult> CompleteRazorpay(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature, int productId, int addressId)
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            // (Optional): You can verify the Razorpay signature here for higher security

            // Place order in DB (mark as paid)
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var order = new COD_UPI_OrdersDto
                    {
                        ProductId = productId,
                        UserId = userId,
                        PaymentType = "RAZORPAY",
                        PaymentStatus = "Paid",
                        OrderDate = DateTime.Now,
                        DeliveryDate = DateTime.Now.AddDays(5)
                    };

                    var json = JsonConvert.SerializeObject(order);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = await client.PostAsync("https://localhost:44365/api/orders/place", content);

                    if (res.IsSuccessStatusCode)
                    {
                        // Return a redirect URL to confirmation page
                        return Json(new { success = true, redirectUrl = Url.Action("OrderConfirmed", "Order") });
                    }
                    return Json(new { success = false, message = await res.Content.ReadAsStringAsync() });
                }
            }
        }

        // Confirmation page after payment success
        [HttpGet]
        public ActionResult OrderConfirmed()
        {
            ViewBag.Message = "Order Confirmed!";
            return View("PlaceOrder");
        }

        // Existing UPI/COD Place Order (leave as is for fallback, but add "RAZORPAY" support in server)
        [HttpPost]
        public async Task<ActionResult> PlaceOrder(int productId, int addressId, string method)
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            decimal productCost = 0;
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var res = await client.GetAsync($"https://localhost:44365/api/product/{productId}");
                    if (res.IsSuccessStatusCode)
                    {
                        var json = await res.Content.ReadAsStringAsync();
                        var product = JsonConvert.DeserializeObject<ProductDto>(json);
                        productCost = (decimal)product.Cost;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to fetch product price.");
                        return View("ChoosePayment");
                    }
                }
            }

            decimal deliveryFee = 100;
            decimal totalCost = productCost + deliveryFee;

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var order = new COD_UPI_OrdersDto
                    {
                        ProductId = productId,
                        UserId = userId,
                        PaymentType = method.ToUpper(), // "COD" or "UPI"
                        PaymentStatus = method.ToUpper() == "COD" ? "Pending" : "Paid",
                        OrderDate = DateTime.Now,
                        DeliveryDate = DateTime.Now.AddDays(5)
                    };

                    var json = JsonConvert.SerializeObject(order);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = await client.PostAsync("https://localhost:44365/api/orders/place", content);

                    if (res.IsSuccessStatusCode)
                    {
                        ViewBag.Message = "Order Confirmed!";
                        return View("PlaceOrder");
                    }

                    ModelState.AddModelError("", await res.Content.ReadAsStringAsync());
                    return View("ChoosePayment");
                }
            }
        }
    }
}