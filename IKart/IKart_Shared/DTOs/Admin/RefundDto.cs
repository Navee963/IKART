using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Admin
{
    public class RefundDto
    {
        public int RefundId { get; set; }
        public int PaymentId { get; set; }

        // Refund amount
        public decimal Amount { get; set; }

        public string Reason { get; set; }
        public string Status { get; set; }

        // Refund date
        public DateTime RefundDate { get; set; }

        // Related User
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }

        // Related Product
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        // Payment Details
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; }
    }
}
