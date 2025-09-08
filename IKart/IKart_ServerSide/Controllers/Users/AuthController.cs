// ⭐ FULL UPDATED AuthController (Web API)

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using IKart_ServerSide.Models;
using IKart_Shared.DTOs.Authentication;
using IKart_ServerSide.Services;
using System.Net;

namespace IKart_ServerSide.Controllers.Users
{
    [RoutePrefix("api/user/auth")]
    public class AuthController : ApiController
    {
        IKartEntities db = new IKartEntities();
        private readonly EmailService _emailService = new EmailService();

        // ✅ Register
        [HttpPost]
        [Route("register")]
        public async Task<IHttpActionResult> Register([FromBody] UserRegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid input");

            if (db.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email already exists");

            if (db.Users.Any(u => u.PhoneNo == dto.PhoneNo))
                return BadRequest("Phone number already exists");

            if (db.Users.Any(u => u.Username == dto.Username))
                return BadRequest("Username already exists");

            // generate OTP
            var otp = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.Now.AddMinutes(5);

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNo = dto.PhoneNo,
                Username = dto.Username,
                PasswordHash = dto.Password, // ⚠️ Use hashing in production
                Status = "Pending",
                OTP = otp,
                OtpExpiry = expiry,
                IsVerified = false,
                CreatedDate = DateTime.Now
            };

            db.Users.Add(user);
            db.SaveChanges();

            // send OTP via email (async)
            await _emailService.SendEmailAsync(user.Email, "OTP Verification", $"Your OTP is <b>{otp}</b>. It is valid for 5 minutes.");

            return Ok(new { message = "OTP sent to your email", user.UserId });
        }

        // ✅ Verify OTP
        [HttpPost]
        [Route("verify-otp")]
        public IHttpActionResult VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var user = db.Users.FirstOrDefault(u => u.UserId == dto.UserId);
            if (user == null)
                return BadRequest("User not found");

            if (user.OTP != dto.Otp || user.OtpExpiry < DateTime.Now)
                return BadRequest("Invalid or expired OTP");

            user.IsVerified = true;
            user.Status = "Active";
            user.OTP = null;
            user.OtpExpiry = null;
            db.SaveChanges();

            return Ok(new { message = "Account verified successfully" });
        }

        // ✅ Resend OTP
        [HttpPost]
        [Route("resend-otp")]
        public async Task<IHttpActionResult> ResendOtp([FromBody] int userId)
        {
            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound();

            if (user.IsVerified==true)
                return BadRequest("User already verified");

            // generate new OTP
            var otp = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.Now.AddMinutes(5);

            user.OTP = otp;
            user.OtpExpiry = expiry;
            db.SaveChanges();

            await _emailService.SendEmailAsync(user.Email, "OTP Verification", $"Your new OTP is <b>{otp}</b>. It is valid for 5 minutes.");

            return Ok(new { message = "New OTP sent to your email" });
        }

        //forgot Password
        [HttpPost]
        [Route("forgot-password")]
        public async Task<IHttpActionResult> ForgotPassword([FromBody] string email)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return BadRequest("Email not found");

            var otp = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.Now.AddMinutes(5);

            user.OTP = otp;
            user.OtpExpiry = expiry;
            db.SaveChanges();

            await _emailService.SendEmailAsync(user.Email, "Password Reset OTP", $"Your OTP is <b>{otp}</b>. It is valid for 5 minutes.");

            return Ok(new { message = "OTP sent to your email", user.UserId });
        }

        //verify otp

        [HttpPost]
        [Route("verify-reset-otp")]
        public IHttpActionResult VerifyResetOtp([FromBody] VerifyOtpDto dto)
        {
            var user = db.Users.FirstOrDefault(u => u.UserId == dto.UserId);
            if (user == null)
                return BadRequest("User not found");

            if (user.OTP != dto.Otp || user.OtpExpiry < DateTime.Now)
                return BadRequest("Invalid or expired OTP");

            user.OTP = null;
            user.OtpExpiry = null;
            db.SaveChanges();

            return Ok(new { message = "OTP verified. You can now reset your password." });
        }

        //reset-Password

        [HttpPost]
        [Route("reset-password")]
        public IHttpActionResult ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = db.Users.FirstOrDefault(u => u.UserId == dto.UserId);
            if (user == null)
                return BadRequest("User not found");

            user.PasswordHash = dto.NewPassword; // ⚠️ Hash in production
            db.SaveChanges();

            return Ok(new { message = "Password reset successful" });
        }



        // ✅ Login
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login([FromBody] UserLoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and Password are required");

            var user = db.Users
                .FirstOrDefault(u => u.Username == dto.Username && u.PasswordHash == dto.Password);

            if (user == null)
                return Unauthorized();

            if (user.IsVerified != true)
            {
                // ⭐ UPDATED → return UserId also
                return Content(HttpStatusCode.BadRequest,
                    new { message = "Please verify your account with OTP before login", user.UserId });
            }

            return Ok(new { message = "Login successful", user.UserId, user.FullName, user.Username });
        }
    }
}
