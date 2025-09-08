using IKart_Shared.DTOs.Admin;

using Newtonsoft.Json;

using System.Collections.Generic;

using System.Net.Http;

using System.Text;

using System.Threading.Tasks;

using System.Web.Mvc;

namespace IKart_Client.Controllers.User

{

    public class SupportController : Controller

    {

        private readonly string apiUrl = "https://localhost:44365/api/supporttickets";

        // ✅ Show form

        public ActionResult Index()

        {

            return View();

        }

        //// ✅ Submit form

        //[HttpPost]

        //public async Task<ActionResult> Index(SupportTicketDto model)

        //{

        //    if (!ModelState.IsValid)

        //        return View(model);

        //    using (var handler = new HttpClientHandler())

        //    {

        //        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

        //        using (HttpClient client = new HttpClient(handler))

        //        {

        //            var json = JsonConvert.SerializeObject(model);

        //            var content = new StringContent(json, Encoding.UTF8, "application/json");

        //            var response = await client.PostAsync(apiUrl, content);

        //            if (response.IsSuccessStatusCode)

        //            {

        //                TempData["Success"] = "Your support ticket has been raised successfully!";

        //                return RedirectToAction("Index");

        //            }

        //            else

        //            {

        //                TempData["Error"] = "Something went wrong, please try again.";

        //            }

        //        }

        //    }

        //    return View(model);

        //}

        [HttpPost]

        public async Task<ActionResult> Index(SupportTicketDto model)

        {

            if (!ModelState.IsValid)

                return View(model);

            using (var handler = new HttpClientHandler())

            {

                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (HttpClient client = new HttpClient(handler))

                {

                    var json = JsonConvert.SerializeObject(model);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(apiUrl + "/create", content);

                    if (response.IsSuccessStatusCode)

                    {

                        TempData["Success"] = "Your support ticket has been raised successfully!";

                        return RedirectToAction("Index");

                    }

                    else

                    {

                        TempData["Error"] = "Something went wrong, please try again.";

                    }

                }

            }

            return View(model);

        }

        //// ✅ View ticket status

        //public async Task<ActionResult> Status(string username)

        //{

        //    List<dynamic> tickets = new List<dynamic>();

        //    using (var handler = new HttpClientHandler())

        //    {

        //        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

        //        using (HttpClient client = new HttpClient(handler))

        //        {

        //            var response = await client.GetAsync(apiUrl + "/status/" + username);

        //            if (response.IsSuccessStatusCode)

        //            {

        //                var json = await response.Content.ReadAsStringAsync();

        //                tickets = JsonConvert.DeserializeObject<List<dynamic>>(json);

        //            }

        //        }

        //    }

        //    return View(tickets);

        //}

        // ✅ View Incident Status page

        public async Task<ActionResult> IncidentStatus()

        {

            List<dynamic> tickets = new List<dynamic>();

            var username = Session["Username"]?.ToString(); // Make sure user is logged in

            if (string.IsNullOrEmpty(username))

                return RedirectToAction("Login", "Account");

            using (var handler = new HttpClientHandler())

            {

                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (HttpClient client = new HttpClient(handler))

                {

                    var response = await client.GetAsync(apiUrl + "/status/" + username);

                    if (response.IsSuccessStatusCode)

                    {

                        var json = await response.Content.ReadAsStringAsync();

                        tickets = JsonConvert.DeserializeObject<List<dynamic>>(json);

                    }

                }

            }

            return View(tickets);

        }


    }

}

