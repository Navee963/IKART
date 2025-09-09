using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Orders
{
    public class OrderDto
    {
        public int Order_Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int? PaymentId { get; set; }

        //public int AddressId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
    }
    public class COD_UPI_OrdersDto
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int AddressId { get; set; } // <-- Add this
        public string PaymentType { get; set; } // "COD" or "UPI"
        public string PaymentStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
    }
}
