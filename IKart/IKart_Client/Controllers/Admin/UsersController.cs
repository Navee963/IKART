using IKart_Shared.DTOs.Admin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;

namespace IKart_Client.Controllers
{
    public class UsersController : Controller
    {
        private readonly string apiBaseUrl = "https://localhost:44365/api/users";

        // GET: Users
        public ActionResult Index()
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var response = client.GetAsync(apiBaseUrl).Result; // call API
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("API call failed: " + response.StatusCode);
                    }

                    var json = response.Content.ReadAsStringAsync().Result;
                    var users = JsonConvert.DeserializeObject<List<UserDto>>(json);
                    return View(users);
                }
            }
        }


        // GET: Users/View/5
        public ActionResult View(int id)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var response = client.GetStringAsync($"{apiBaseUrl}/{id}").Result;
                    var user = JsonConvert.DeserializeObject<UserDto>(response);
                    return View(user);
                }
            }
        }

        // POST: Users/UpdateStatus/5
        [HttpPost]
        public ActionResult UpdateStatus(int id, string status)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var content = new StringContent($"\"{status}\"", Encoding.UTF8, "application/json");
                    var response = client.PutAsync($"{apiBaseUrl}/{id}/status", content).Result;

                    if (response.IsSuccessStatusCode)
                        TempData["Message"] = "Status updated successfully!";
                }
            }

            return RedirectToAction("Index");
        }
    }
}
