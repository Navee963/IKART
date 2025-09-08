using IKart_Shared.DTOs.Payment;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
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
                using (HttpClient client = new HttpClient(handler))
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
                using (HttpClient client = new HttpClient(handler))
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

        [HttpPost]
        public ActionResult PayInstallment(int installmentId, int paymentId)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (s, c, ch, e) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var res = client.PostAsync($"{apiBase}/pay-installment/{installmentId}", null).Result;
                    if (res.IsSuccessStatusCode)
                        TempData["Success"] = "Installment paid successfully.";
                    else
                        TempData["Error"] = "Payment failed.";
                }
            }

            return RedirectToAction("Details", new { paymentId });
        }
    }
}