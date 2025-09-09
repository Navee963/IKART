using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using IKart_Shared.DTOs;

namespace IKart_Client.Controllers
{
    public class ProductSearchController : Controller
    {
        private readonly string apiUrl = "https://localhost:44365/api/search/products";

        // GET: ProductSearch/Products
        public async Task<ActionResult> Products(string query = "")
        {
            List<ProductDto> products = new List<ProductDto>();

            try
            {
                var handler = new HttpClientHandler
                {
                    // ⚠️ Dev only - ignore self-signed cert errors
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    var response = await client.GetAsync($"{apiUrl}?query={Uri.EscapeDataString(query ?? "")}");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<List<ProductDto>>(json);

                        if (result != null)
                            products = result;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Search API Error: " + ex.Message);
                products = new List<ProductDto>();
            }

            ViewBag.Query = query; // ✅ pass query to view
            return View(products ?? new List<ProductDto>());
        }
    }
}