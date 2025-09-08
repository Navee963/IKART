using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IKart_Shared.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, RegularExpression(@"^[0-9]{10,15}$", ErrorMessage = "Phone number must be 10–15 digits")]
        public string PhoneNo { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; }

        // Only for password reset
        public string Password { get; set; }
    }

}