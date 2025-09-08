using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using IKart_Shared.DTOs;
using IKart_Shared.DTOs.Admin;
using Newtonsoft.Json;

namespace IKart_Client.Controllers
{
    public class RefundsController : Controller
    {
        private readonly string apiBaseUrl = "https://localhost:44365/api/refunds";

        // GET: Refunds
        public ActionResult Index()
        {
            List<RefundDto> refunds = new List<RefundDto>();

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var response = client.GetStringAsync(apiBaseUrl).Result;
                    refunds = JsonConvert.DeserializeObject<List<RefundDto>>(response);
                }
            }

            return View(refunds);
        }

        // GET: Refunds/View/5
        public ActionResult View(int id)
        {
            RefundDto refund = null;

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var response = client.GetStringAsync($"{apiBaseUrl}/{id}").Result;
                    refund = JsonConvert.DeserializeObject<RefundDto>(response);
                }
            }

            if (refund == null)
            {
                return HttpNotFound();
            }

            return View(refund);
        }
    }
}