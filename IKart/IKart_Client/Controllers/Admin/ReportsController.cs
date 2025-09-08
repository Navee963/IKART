using System.Net.Http;
using System.Web.Mvc;
using IKart_Shared.DTOs;
using Newtonsoft.Json;
using System.Collections.Generic;
using IKart_Shared.DTOs.Admin;

namespace IKart_Client.Controllers
{
    public class ReportsController : Controller
    {
        private readonly string apiBaseUrl = "https://localhost:44365/api/reports";

        public ActionResult Index()
        {
            ReportsDto reports = null;

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var response = client.GetStringAsync(apiBaseUrl).Result;
                    reports = JsonConvert.DeserializeObject<ReportsDto>(response);
                }
            }

            return View(reports);
        }
    }
}
