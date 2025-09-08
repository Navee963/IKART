using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Net.Http;
using IKart_Shared.DTOs;
using Newtonsoft.Json;

namespace IKart_Client.Controllers
{
    public class ProductController : Controller
    {
        string baseUrl = "https://localhost:44365/api/product";

        // GET: Product
        public ActionResult Index()
        {
            List<ProductDto> products = new List<ProductDto>();
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var res = client.GetAsync($"{baseUrl}/all").Result;
                    if (res.IsSuccessStatusCode)
                    {
                        var data = res.Content.ReadAsStringAsync().Result;
                        products = JsonConvert.DeserializeObject<List<ProductDto>>(data);
                    }
                }
            }

            return View(products);
        }

        // GET: Product/Details/5
        public ActionResult Details(int id)
        {
            ProductDto product = null;
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    var res = client.GetAsync($"{baseUrl}/{id}").Result;
                    if (res.IsSuccessStatusCode)
                    {
                        var data = res.Content.ReadAsStringAsync().Result;
                        product = JsonConvert.DeserializeObject<ProductDto>(data);
                    }
                }
            }

            if (product == null)
                return HttpNotFound();

            return View(product);
        }
    }
}