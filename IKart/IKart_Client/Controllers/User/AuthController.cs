// ⭐ FULL UPDATED AuthController (MVC)

using IKart_Shared.DTOs.Authentication;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace IKart_ClientSide.Controllers.User
{
    public class AuthController : Controller
    {
        private readonly string apiBase = "https://localhost:44365/api/user/auth/";

        // Register Page
        public ActionResult Register() => View();

        [HttpPost]
        public async Task<ActionResult> Register(UserRegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var res = await client.PostAsJsonAsync(apiBase + "register", dto);

                    if (res.IsSuccessStatusCode)
                    {
                        var result = await res.Content.ReadAsAsync<dynamic>();
                        TempData["UserId"] = result.UserId;
                        return RedirectToAction("VerifyOtp");
                    }

                    ModelState.AddModelError("", await res.Content.ReadAsStringAsync());
                    return View(dto);
                }
            }
        }

        // OTP Verification
        public ActionResult VerifyOtp()
        {
            if (TempData["UserId"] == null)
                return RedirectToAction("Register");

            ViewBag.UserId = TempData["UserId"];
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> VerifyOtp(int userId, string otp)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var dto = new VerifyOtpDto { UserId = userId, Otp = otp };
                    var res = await client.PostAsJsonAsync(apiBase + "verify-otp", dto);

                    if (res.IsSuccessStatusCode)
                        return RedirectToAction("Login");

                    ModelState.AddModelError("", await res.Content.ReadAsStringAsync());
                    ViewBag.UserId = userId;
                    return View();
                }
            }
        }

        // Login Page
        public ActionResult Login() => View();

        [HttpPost]
        public async Task<ActionResult> Login(UserLoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var res = await client.PostAsJsonAsync(apiBase + "login", dto);

                    if (res.IsSuccessStatusCode)
                    {
                        var result = await res.Content.ReadAsAsync<dynamic>();
                        Session["UserId"] = result.UserId;
                        Session["FullName"] = result.FullName;
                        Session["Username"] = result.Username;
                        return RedirectToAction("Index", "Product");
                    }
                    else
                    {
                        var errorContent = await res.Content.ReadAsStringAsync();

                        if (!string.IsNullOrWhiteSpace(errorContent))
                        {
                            dynamic errorJson = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(errorContent);
                            string message = errorJson?.message?.ToString() ?? "Invalid username or password";

                            if (message.Contains("verify your account"))
                            {
                                ViewBag.ShowResendOtp = true;
                                ViewBag.UserId = (int)(errorJson?.UserId ?? 0);
                            }

                            ModelState.AddModelError("", message);
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid username or password");
                        }

                        return View(dto);
                    }
                }
            }
        }


        //ForgotPassword

        public ActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Email is required.");
                return View();
            }

            using (var handler = new HttpClientHandler())
            {
                // ✅ Bypass SSL validation for local dev
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri(apiBase);
                    var res = await client.PostAsJsonAsync("forgot-password", email);

                    if (res.IsSuccessStatusCode)
                    {
                        var result = await res.Content.ReadAsAsync<dynamic>();
                        TempData["UserId"] = result.UserId;
                        return RedirectToAction("VerifyResetOtp");
                    }

                    ModelState.AddModelError("", await res.Content.ReadAsStringAsync());
                    return View();
                }
            }
        }


        //VerifyResetOtp

        public ActionResult VerifyResetOtp()
        {
            if (TempData["UserId"] == null) return RedirectToAction("ForgotPassword");
            ViewBag.UserId = TempData["UserId"];
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> VerifyResetOtp(int userId, string otp)
        {
            using (var handler = new HttpClientHandler())
            {
                // ✅ Bypass SSL validation for local development
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var dto = new VerifyOtpDto { UserId = userId, Otp = otp };
                    var res = await client.PostAsJsonAsync(apiBase + "verify-reset-otp", dto);

                    if (res.IsSuccessStatusCode)
                    {
                        TempData["UserId"] = userId;
                        return RedirectToAction("ResetPassword");
                    }

                    ModelState.AddModelError("", await res.Content.ReadAsStringAsync());
                    ViewBag.UserId = userId;
                    return View();
                }
            }
        }


        //ResetPassword

        public ActionResult ResetPassword()
        {
            if (TempData["UserId"] == null) return RedirectToAction("ForgotPassword");
            ViewBag.UserId = TempData["UserId"];
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ResetPassword(int userId, string newPassword)
        {
            using (var handler = new HttpClientHandler())
            {
                // ✅ Bypass SSL certificate validation for development
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var dto = new ResetPasswordDto
                    {
                        UserId = userId,
                        NewPassword = newPassword
                    };

                    var res = await client.PostAsJsonAsync(apiBase + "reset-password", dto);

                    if (res.IsSuccessStatusCode)
                        return RedirectToAction("Login");

                    ModelState.AddModelError("", await res.Content.ReadAsStringAsync());
                    ViewBag.UserId = userId;
                    return View();
                }
            }
        }




        // ✅ Resend OTP (MVC)
        [HttpPost]
        public async Task<ActionResult> ResendOtp(int userId)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var res = await client.PostAsJsonAsync(apiBase + "resend-otp", userId);
                    if (res.IsSuccessStatusCode)
                    {
                        TempData["UserId"] = userId;
                        return RedirectToAction("VerifyOtp");
                    }

                    ModelState.AddModelError("", await res.Content.ReadAsStringAsync());
                    return RedirectToAction("Login");
                }
            }
        }
    }
}
