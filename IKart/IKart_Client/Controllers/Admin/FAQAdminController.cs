using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using IKart_Shared.DTOs;
using Newtonsoft.Json;

namespace IKart_Client.Controllers.Admin
{
    public class FAQAdminController : Controller
    {
        private readonly string apiUrl = "https://localhost:44365/api/faq";
        private readonly string productApiUrl = "https://localhost:44365/api/products";

        private HttpClient GetHttpClient()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
            };
            return new HttpClient(handler);
        }

        public async Task<ActionResult> SelectProduct()
        {
            var products = new List<ProductDto>();
            using (var client = GetHttpClient())
            {
                var response = await client.GetAsync(productApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    products = JsonConvert.DeserializeObject<List<ProductDto>>(json) ?? new List<ProductDto>();
                }
            }
            return View(products);
        }

        public async Task<ActionResult> Index(int? productId)
        {
            if (!productId.HasValue) return RedirectToAction("SelectProduct");

            List<FAQDto> faqs = new List<FAQDto>();
            using (var client = GetHttpClient())
            {
                var response = await client.GetAsync($"{apiUrl}/product/{productId.Value}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    faqs = JsonConvert.DeserializeObject<List<FAQDto>>(json) ?? new List<FAQDto>();
                }
            }

            // Optional: filter out nulls and duplicates
            faqs = faqs
                .Where(f => f != null)
                .GroupBy(f => f.Question.Trim())
                .Select(g => g.First())
                .ToList();

            ViewBag.ProductId = productId.Value;
            return View(faqs);
        }


        public ActionResult Add(int productId) => View(new FAQDto { ProductId = productId });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(FAQDto faq)
        {
            if (!ModelState.IsValid) return View(faq);

            using (var client = GetHttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(faq), Encoding.UTF8, "application/json");
                await client.PostAsync($"{apiUrl}/add", content);
            }

            TempData["Message"] = "FAQ added successfully!";
            return RedirectToAction("Index", new { productId = faq.ProductId });
        }

        public async Task<ActionResult> Edit(int id)
        {
            FAQDto faq = null;
            using (var client = GetHttpClient())
            {
                var response = await client.GetAsync($"{apiUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    faq = JsonConvert.DeserializeObject<FAQDto>(json);
                }
            }

            if (faq == null) return HttpNotFound();
            return View(faq);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(FAQDto faq)
        {
            if (!ModelState.IsValid) return View(faq);

            using (var client = GetHttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(faq), Encoding.UTF8, "application/json");
                await client.PutAsync($"{apiUrl}/update/{faq.FaqId}", content);
            }

            TempData["Message"] = "FAQ updated successfully!";
            return RedirectToAction("Index", new { productId = faq.ProductId });
        }

        public async Task<ActionResult> Delete(int id, int productId)
        {
            using (var client = GetHttpClient())
            {
                await client.DeleteAsync($"{apiUrl}/delete/{id}");
            }

            TempData["Message"] = "FAQ deleted successfully!";
            return RedirectToAction("Index", new { productId });
        }
    }
}