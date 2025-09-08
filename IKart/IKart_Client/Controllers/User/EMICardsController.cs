using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using IKart_Shared.DTOs.EMI_Card;

namespace IKart_Client.Controllers.User
{
    public class EMICardsController : Controller
    {
        // Show existing EMI cards
        public ActionResult Index()
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (var client = new HttpClient(handler))
                {
                    var response = client.GetAsync($"https://localhost:44365/api/emicards/user/{userId}").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var data = response.Content.ReadAsAsync<List<IKart_Shared.DTOs.EMI_Card.EmiCardDto>>().Result;
                        return View(data);
                    }
                }
            }
            return View(new List<IKart_Shared.DTOs.EMI_Card.EmiCardDto>());
        }

        // Select Card Type
        public ActionResult AddCard()
        {
            return View();
        }

        // Submit Card Request and temporarily store files
        [HttpPost]
        public ActionResult RequestCard(CardRequestDto dto)
        {
            if (Session["UserId"] == null)
            {
                TempData["Error"] = "Please login first.";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
                return View("AddCard", dto);

            dto.UserId = Convert.ToInt32(Session["UserId"]);

            // Store request temporarily in session
            Session["PendingCardRequest"] = dto;

            // Save uploaded documents temporarily to a folder
            var files = HttpContext.Request.Files;
            var tempDocs = new List<string>();
            var tempPath = Server.MapPath("~/TempDocs/");
            Directory.CreateDirectory(tempPath);

            foreach (string key in files)
            {
                var file = files[key];
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var fullPath = Path.Combine(tempPath, fileName);
                    file.SaveAs(fullPath);
                    tempDocs.Add(fileName);
                }
            }

            Session["PendingDocuments"] = tempDocs;

            // Determine joining fee (FIXED: Gold = 1000)
            decimal fee;
            switch (dto.CardType)
            {
                case "Gold": fee = 1000; break;
                case "Diamond": fee = 2000; break;
                default: fee = 3000; break;
            }
            Session["FeeAmount"] = fee;

            return RedirectToAction("Index", "Payment");
        }
    }
}