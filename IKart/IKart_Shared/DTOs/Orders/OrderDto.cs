using System;

namespace IKart_Shared.DTOs.Orders
{
    public class OrderDto
    {
        public int Order_Id { get; set; }      // Primary key
        public int ProductId { get; set; }     // Product being ordered
        public int UserId { get; set; }        // User who placed the order
        public int? PaymentId { get; set; }    // Nullable, in case payment is pending

        //public int AddressId { get; set; }   // Uncomment if address is required

        public DateTime OrderDate { get; set; }     // When the order was placed
        public DateTime DeliveryDate { get; set; }  // Expected or actual delivery date
    }

    public class COD_UPI_OrdersDto
    {
        public int ProductId { get; set; }     // Product being ordered
        public int UserId { get; set; }        // User placing the order
        public string PaymentType { get; set; } // "COD" or "UPI"
        public string PaymentStatus { get; set; } // Optional: "Pending", "Paid", etc.
        public DateTime OrderDate { get; set; }    // Order date
        public DateTime DeliveryDate { get; set; } // Delivery date
    }

    public class UserOrderDto
    {
        public int OrderId { get; set; }
        public string ProductName { get; set; }
        public string PaymentType { get; set; } // "Online", "COD", "UPI"
        public string PaymentStatus { get; set; } // Optional
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }

    }
}
