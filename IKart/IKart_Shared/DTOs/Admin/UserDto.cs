using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Admin
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }   // maps from Username (DB)
        public string FullName { get; set; }   // optional, if you also want FullName
        public string Email { get; set; }
        public string PhoneNumber { get; set; } // maps from PhoneNo (DB)
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
    }
}
