using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Payment
{
    public class UserPaymentDto
    {
        // Basic Payment Info
        public int PaymentId { get; set; }
        public string ProductName { get; set; }
        public string CardType { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }

        // EMI Breakdown
        public decimal EMIAmount { get; set; }  // Renamed to match controller
        public int TenureMonths { get; set; }

        // Installments
        public List<InstallmentDto> Installments { get; set; }
    }

    public class InstallmentDto
    {
        public int InstallmentId { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }

        public PenaltyDto Penalty { get; set; }

    }

    public class PenaltyDto
    {
        public int PenaltyId { get; set; }
        public int Days_Overdue { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string Status { get; set; }
    }
}
