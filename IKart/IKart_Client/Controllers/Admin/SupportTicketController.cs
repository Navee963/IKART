using System.Collections.Generic;

using System.Net.Http;

using System.Threading.Tasks;

using System.Web.Mvc;
using IKart_ClientSide.Filters;
using IKart_Shared.DTOs;

using IKart_Shared.DTOs.Admin;

using Newtonsoft.Json;

namespace IKart_Client.Controllers

{
    [AdminAuthorize]

    public class SupportTicketsController : Controller

    {

        private readonly string apiUrl = "https://localhost:44365/api/supporttickets";

        public async Task<ActionResult> Index()

        {

            List<SupportTicketDto> tickets = new List<SupportTicketDto>();

            using (var handler = new HttpClientHandler())

            {

                // ✅ ignore SSL certificate errors (dev only!)

                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (HttpClient client = new HttpClient(handler))

                {

                    var response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)

                    {

                        var json = await response.Content.ReadAsStringAsync();

                        tickets = JsonConvert.DeserializeObject<List<SupportTicketDto>>(json);

                    }

                }

            }

            return View(tickets);

        }

        [HttpPost]

        public async Task<ActionResult> CloseTicket(int id)

        {

            using (var handler = new HttpClientHandler())

            {

                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (HttpClient client = new HttpClient(handler))

                {

                    var response = await client.PutAsync(apiUrl + "/close/" + id, null);

                }

            }

            return RedirectToAction("Index");

        }

    }

}

