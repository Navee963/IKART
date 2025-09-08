using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs.EMI_Card
{
    public class PaymentDto
    {
        public int CardId { get; set; }
        public int UserId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentId { get; set; }
    }
}
