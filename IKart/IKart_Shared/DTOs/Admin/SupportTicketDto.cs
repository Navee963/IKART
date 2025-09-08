using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Admin
{
    public class SupportTicketDto
    {
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }  // ✅ Add username for mapping
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
    }
}
