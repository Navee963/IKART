using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IKart_Shared.DTOs.Authentication
{
    public class UserLoginDto
    {
        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required, StringLength(100)]
        public string Password { get; set; }
    }
}
