using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using IKart_Shared.DTOs;
using Newtonsoft.Json;

namespace IKart_Client.Controllers
{
    public class FAQController : Controller
    {
        private readonly string apiUrl = "https://localhost:44365/api/faq";

        private HttpClient GetHttpClient()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
            };
            return new HttpClient(handler);
        }


        public async Task<ActionResult> ProductFAQs(int productId)
        {
            List<FAQDto> faqs = new List<FAQDto>();

            using (var client = GetHttpClient())
            {
                var response = await client.GetAsync($"{apiUrl}/product/{productId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    faqs = JsonConvert.DeserializeObject<List<FAQDto>>(json) ?? new List<FAQDto>();
                }
            }

            ViewBag.ProductId = productId;
            return View(faqs); // always pass all FAQs
        }


    }
}