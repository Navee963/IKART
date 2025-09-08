using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Admin
{
    // Main Reports DTO
    public class ReportsDto
    {
        // EMI Cards
        public int TotalEmiCards { get; set; }
        public Dictionary<string, int> EmiCardsByType { get; set; }
        public List<ReportEmiCardDto> EmiCards { get; set; }

        // Users
        public int TotalUsers { get; set; }
        public List<ReportUserDto> Users { get; set; }

        // Orders
        public int TotalOrders { get; set; }
        public List<ReportOrderDto> Orders { get; set; }
        public Dictionary<string, int> OrdersByRegion { get; set; }
        public Dictionary<string, int> OrdersByCategory { get; set; }
        public Dictionary<string, int> OrdersByPayment { get; set; }

        // Refunds
        public int TotalRefunds { get; set; }
        public List<ReportRefundDto> Refunds { get; set; }

        // Returns
        public int TotalReturns { get; set; }
        public List<ReportReturnDto> Returns { get; set; }

        // Cancellations
        public int TotalCancellations { get; set; }
        public List<ReportCancellationDto> Cancellations { get; set; }

        public ReportsDto()
        {
            EmiCardsByType = new Dictionary<string, int>();
            EmiCards = new List<ReportEmiCardDto>();
            Users = new List<ReportUserDto>();
            Orders = new List<ReportOrderDto>();
            OrdersByRegion = new Dictionary<string, int>();
            OrdersByCategory = new Dictionary<string, int>();
            OrdersByPayment = new Dictionary<string, int>();
            Refunds = new List<ReportRefundDto>();
            Returns = new List<ReportReturnDto>();
            Cancellations = new List<ReportCancellationDto>();
        }
    }

    // EMI Card details DTO
    public class ReportEmiCardDto
    {
        public int EmiCardId { get; set; }
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public decimal TotalLimit { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public string CardImage { get; set; }
    }

    // User DTO
    public class ReportUserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string Username { get; set; }
        public bool Status { get; set; }        // Active or not
        public DateTime CreatedDate { get; set; }
    }

    // Order DTO
    public class ReportOrderDto
    {
        public int OrderId { get; set; }
        public string UserFullName { get; set; }
        public string ProductName { get; set; }
        public string Region { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string CategoryName { get; set; }
    }

    // Refund DTO
    public class ReportRefundDto
    {
        public int RefundId { get; set; }
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime RefundDate { get; set; }
    }

    // Return DTO
    public class ReportReturnDto
    {
        public int ReturnId { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime ReturnDate { get; set; }
    }

    // Cancellation DTO
    public class ReportCancellationDto
    {
        public int CancellationId { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string Reason { get; set; }
        public DateTime CancelDate { get; set; }
        public bool Refunded { get; set; }
    }
}
