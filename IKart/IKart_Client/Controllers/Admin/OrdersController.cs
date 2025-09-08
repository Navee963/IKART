using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IKart_Shared.DTOs;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using IKart_Shared.DTOs.Admin;

namespace IKart_Client.Controllers
{
    public class OrdersController : Controller
    {
        private readonly string apiBase = "https://localhost:44365/api/orders"; // Use the actual port of server

        public async Task<ActionResult> Index()
        {
            using (var handler = new HttpClientHandler())

            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (var client = new HttpClient(handler))
                {
                    var response = await client.GetStringAsync(apiBase);
                    var orders = JsonConvert.DeserializeObject<List<OrderDto>>(response);
                    return View(orders);
                }
            }
        }

        public async Task<ActionResult> Details(int id)
        {
            using (var handler = new HttpClientHandler())

            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var response = await client.GetStringAsync($"{apiBase}/{id}");
                    var order = JsonConvert.DeserializeObject<OrderDetailsDto>(response);
                    return View(order);
                }
            }
        }
    }
}