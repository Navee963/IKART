using System;
using System.Collections.Generic;

namespace IKart_Shared.DTOs.Payment
{
    public class UserPaymentDto
    {
        public int PaymentId { get; set; }
        public string ProductName { get; set; }
        public string CardType { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
        public int TenureMonths { get; set; }
        public decimal EMIAmount { get; set; }
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
        public decimal PenaltyAmount { get; set; }
        public int Days_Overdue { get; set; }
    }
}