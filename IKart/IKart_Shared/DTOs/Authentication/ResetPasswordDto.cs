using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Authentication
{
    public class ResetPasswordDto
    {
        [Required]
        public int UserId { get; set; }

        [Required, StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Required, Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
