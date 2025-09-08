using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Admin
{
    public class OrderDto
    {
        public int Order_Id { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
    }

    public class OrderDetailsDto
    {
        public int Order_Id { get; set; }
        public string UserName { get; set; }
        public string ProductName { get; set; }
        public decimal ProductCost { get; set; }
        public string PaymentType { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public int TenureMonths { get; set; }
        public int MonthsRemaining { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public List<PaymentHistoryDto> Payments { get; set; }
    }

    public class PaymentHistoryDto
    {
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}
