using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;
using IKart_Shared.DTOs.Admin;
using Newtonsoft.Json;

namespace IKart_Client.Controllers
{
    public class StocksController : Controller
    {
        string apiUrl = "https://localhost:44365/api/stocks";

        // LIST ALL
        public ActionResult Index()
        {
            List<StocksDto> stocks = new List<StocksDto>();
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true;
                using (var client = new HttpClient(handler))
                {
                    var res = client.GetAsync(apiUrl).Result;
                    if (res.IsSuccessStatusCode)
                    {
                        var json = res.Content.ReadAsStringAsync().Result;
                        stocks = JsonConvert.DeserializeObject<List<StocksDto>>(json);
                    }
                }
            }
            return View(stocks);
        }

        // CREATE GET
        public ActionResult Create() => View();

        // CREATE POST
        [HttpPost]
        public ActionResult Create(StocksDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true;
                using (var client = new HttpClient(handler))
                {
                    var json = JsonConvert.SerializeObject(dto);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = client.PostAsync(apiUrl, data).Result;
                    if (res.IsSuccessStatusCode) return RedirectToAction("Index");
                }
            }
            return View(dto);
        }

        // EDIT GET
        public ActionResult Edit(int id)
        {
            StocksDto stock = null;
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true;
                using (var client = new HttpClient(handler))
                {
                    var res = client.GetAsync(apiUrl + "/" + id).Result;
                    if (res.IsSuccessStatusCode)
                    {
                        var json = res.Content.ReadAsStringAsync().Result;
                        stock = JsonConvert.DeserializeObject<StocksDto>(json);
                    }
                }
            }
            return View(stock);
        }

        // EDIT POST
        [HttpPost]
        public ActionResult Edit(int id, StocksDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true;
                using (var client = new HttpClient(handler))
                {
                    var json = JsonConvert.SerializeObject(dto);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = client.PutAsync(apiUrl + "/" + id, data).Result;
                    if (res.IsSuccessStatusCode) return RedirectToAction("Index");
                }
            }
            return View(dto);
        }

        // DELETE GET - Show confirmation page
        public ActionResult Delete(int id)
        {
            StocksDto stock = null;
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true;
                using (var client = new HttpClient(handler))
                {
                    var res = client.GetAsync(apiUrl + "/" + id).Result;
                    if (res.IsSuccessStatusCode)
                    {
                        var json = res.Content.ReadAsStringAsync().Result;
                        stock = JsonConvert.DeserializeObject<StocksDto>(json);
                    }
                }
            }

            if (stock == null) return HttpNotFound();
            return View(stock);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true;
                using (var client = new HttpClient(handler))
                {
                    var res = client.DeleteAsync(apiUrl + "/" + id).Result;

                    if (res.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else if (res.StatusCode == HttpStatusCode.Conflict)
                    {
                        // Read API error message
                        var errorMsg = res.Content.ReadAsStringAsync().Result;
                        ViewBag.Error = errorMsg;

                        // Reload stock details so view can render again
                        var stockRes = client.GetAsync(apiUrl + "/" + id).Result;
                        if (stockRes.IsSuccessStatusCode)
                        {
                            var json = stockRes.Content.ReadAsStringAsync().Result;
                            var stock = JsonConvert.DeserializeObject<StocksDto>(json);
                            return View("Delete", stock);
                        }
                        return View("Delete");
                    }
                }
            }

            // fallback
            return RedirectToAction("Index");
        }
    }
}
