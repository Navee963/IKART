using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IKart_Shared.DTOs.Authentication
{
    public class UserRegisterDto
    {
        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(15, MinimumLength = 10)]
        public string PhoneNo { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required, Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword{ get; set; }

        public bool IsVerified { get; set; }

        public DateTime? OtpExpiry { get; set; }
    }

    public class VerifyOtpDto
    {
        public int UserId { get; set; }
        public string Otp { get; set; }
    }
}
