using System;

using System.Collections.Generic;

using System.Net.Http;

using System.Web.Mvc;

using IKart_Shared.DTOs.Orders;

using Newtonsoft.Json;

namespace IKart_Client.Controllers

{

    public class UserOrdersController : Controller

    {

        string baseUrl = "https://localhost:44365/api/userorders"; // Your API route

        public ActionResult Index()

        {

            int userId = Convert.ToInt32(Session["UserId"]);

            List<UserOrderDto> orders = new List<UserOrderDto>();

            using (var handler = new HttpClientHandler())

            {

                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (HttpClient client = new HttpClient(handler))

                {

                    var res = client.GetAsync($"{baseUrl}/all/{userId}").Result;

                    if (res.IsSuccessStatusCode)

                    {

                        var data = res.Content.ReadAsStringAsync().Result;

                        orders = JsonConvert.DeserializeObject<List<UserOrderDto>>(data);

                    }

                }

            }

            return View(orders);

        }


    }

}

