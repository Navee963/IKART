using IKart_Shared.DTOs.EMI_Card;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace IKart_Client.Controllers.User
{
    public class PaymentController : Controller
    {
        public ActionResult Index()
        {
            if (Session["FeeAmount"] == null)
                return RedirectToAction("Index", "EMICards");

            var fee = (decimal)Session["FeeAmount"];
            var key = ConfigurationManager.AppSettings["RazorpayKey"];
            var secret = ConfigurationManager.AppSettings["RazorpaySecret"];

            RazorpayClient client = new RazorpayClient(key, secret);
            var options = new Dictionary<string, object>
            {
                { "amount", (fee * 100) },
                { "currency", "INR" },
                { "payment_capture", 1 }
            };

            var order = client.Order.Create(options);

            ViewBag.RazorpayKey = key;
            ViewBag.OrderId = order["id"].ToString();
            ViewBag.FeeAmount = fee;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> VerifyPayment(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature, int PaymentMethodId)
        {
            var secret = ConfigurationManager.AppSettings["RazorpaySecret"];
            string generated_signature;
            using (var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret)))
            {
                var rawData = razorpay_order_id + "|" + razorpay_payment_id;
                var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
                generated_signature = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }

            if (generated_signature != razorpay_signature)
            {
                CleanupTempDocs();
                TempData["Error"] = "Payment verification failed.";
                return RedirectToAction("Index", "EMICards");
            }

            var dto = (CardRequestDto)Session["PendingCardRequest"];
            var docFiles = (List<string>)Session["PendingDocuments"];
            var amount = (decimal)Session["FeeAmount"];

            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            using (HttpClient client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri("https://localhost:44365/");

                // 1. Save Card Request
                var response = await client.PostAsJsonAsync("api/emicards/request", dto);
                if (!response.IsSuccessStatusCode)
                {
                    CleanupTempDocs();
                    TempData["Error"] = "Failed to save card request.";
                    return RedirectToAction("Index", "EMICards");
                }

                var result = await response.Content.ReadAsAsync<CardResponse>();
                int cardId = result.dto.Card_Id;

                // 2. Upload documents with correct keys (Aadhaar, PAN, BankBook)
                if (docFiles != null && docFiles.Count == 3) // Ensure all docs present
                {
                    var form = new MultipartFormDataContent();
                    var docFieldNames = new[] { "Aadhaar", "PAN", "BankBook" };
                    for (int i = 0; i < docFiles.Count && i < docFieldNames.Length; i++)
                    {
                        var fileName = docFiles[i];
                        var tempPath = Server.MapPath("~/TempDocs/");
                        var fullPath = Path.Combine(tempPath, fileName);
                        var fileBytes = System.IO.File.ReadAllBytes(fullPath);
                        var streamContent = new ByteArrayContent(fileBytes);
                        streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                        {
                            Name = docFieldNames[i],
                            FileName = fileName
                        };
                        form.Add(streamContent, docFieldNames[i], fileName);
                    }
                    await client.PostAsync($"api/emicards/upload-documents/{cardId}", form);

                    // Move files to permanent location after successful upload
                    MoveDocsToPermanent(docFiles);
                }
                else
                {
                    CleanupTempDocs();
                    TempData["Error"] = "All documents are required.";
                    return RedirectToAction("Index", "EMICards");
                }

                // 3. Pay joining fee
                var paymentDto = new PaymentDto
                {
                    PaymentMethodId = PaymentMethodId,
                    Amount = amount
                };
                await client.PostAsJsonAsync($"api/emicards/payfee/{cardId}", paymentDto);
            }

            CleanupTempDocs();

            TempData["Message"] = "Payment successful!";
            return RedirectToAction("Index", "EMICards");
        }

        private void CleanupTempDocs()
        {
            var docFiles = (List<string>)Session["PendingDocuments"];
            if (docFiles == null) return;

            var tempPath = Server.MapPath("~/TempDocs/");
            foreach (var fileName in docFiles)
            {
                var fullPath = Path.Combine(tempPath, fileName);
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
            Session["PendingDocuments"] = null;
        }

        private void MoveDocsToPermanent(List<string> docFiles)
        {
            var tempPath = Server.MapPath("~/TempDocs/");
            var permPath = Server.MapPath("~/Content/EmiCardDocuments/");
            Directory.CreateDirectory(permPath);

            foreach (var fileName in docFiles)
            {
                var tempFile = Path.Combine(tempPath, fileName);
                var permFile = Path.Combine(permPath, fileName);
                if (System.IO.File.Exists(tempFile))
                    System.IO.File.Move(tempFile, permFile);
            }
        }

        public class CardResponse
        {
            public string message { get; set; }
            public CardRequestDto dto { get; set; }
        }
    }
}