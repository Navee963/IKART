using IKart_Shared.DTOs.Payment;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace IKart_Client.Controllers.User
{
    public class ProductPaymentController : Controller
    {
        private readonly string apiBase = "https://localhost:44365/api/payments";

        public ActionResult UserPaymentPageIndex()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Auth");

            int userId = Convert.ToInt32(Session["UserId"]);
            List<UserPaymentDto> payments = new List<UserPaymentDto>();

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (var client = new HttpClient(handler))
                {
                    var res = client.GetAsync($"{apiBase}/user/{userId}").Result;
                    if (res.IsSuccessStatusCode)
                    {
                        var data = res.Content.ReadAsStringAsync().Result;
                        payments = JsonConvert.DeserializeObject<List<UserPaymentDto>>(data);
                    }
                }
            }

            return View(payments);
        }

        public ActionResult Details(int paymentId)
        {
            UserPaymentDto payment = null;

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (var client = new HttpClient(handler))
                {
                    var res = client.GetAsync($"{apiBase}/details/{paymentId}").Result;
                    if (res.IsSuccessStatusCode)
                    {
                        var data = res.Content.ReadAsStringAsync().Result;
                        payment = JsonConvert.DeserializeObject<UserPaymentDto>(data);
                    }
                }
            }

            return View(payment);
        }

        // New: Razorpay Installment Payment Initiation
        [HttpGet]
        public async Task<ActionResult> InstallmentRazorpay(int installmentId, int paymentId)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (var client = new HttpClient(handler))
                {
                    var response = await client.PostAsync($"{apiBase}/installment-razorpay-order/{installmentId}", null);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        dynamic obj = JsonConvert.DeserializeObject(json);

                        ViewBag.OrderId = obj.orderId;
                        ViewBag.Amount = obj.amount;
                        ViewBag.Currency = obj.currency;
                        ViewBag.InstallmentId = obj.installmentId;
                        ViewBag.PaymentId = paymentId;
                        return View("InstallmentRazorpayPayment");
                    }
                    else
                    {
                        TempData["Error"] = "Unable to initiate Razorpay payment for installment.";
                        return RedirectToAction("Details", new { paymentId });
                    }
                }
            }
        }



        // New: Razorpay Installment Payment Verification
        [HttpPost]
        public async Task<ActionResult> VerifyInstallmentPayment(VerifyPaymentDto dto, int paymentId)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (var client = new HttpClient(handler))
                {
                    var json = JsonConvert.SerializeObject(dto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = await client.PostAsync($"{apiBase}/installment-razorpay-verify", content);

                    if (res.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Installment paid successfully!";
                    }
                    else
                    {
                        var error = await res.Content.ReadAsStringAsync();
                        TempData["Error"] = error;
                    }
                    return RedirectToAction("UserPaymentPageIndex", new { paymentId = paymentId });
                }
            }
        }
    }
}