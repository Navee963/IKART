using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Payment

{

    // For generic payments (Credit Card, Debit Card, UPI, etc.)

    public class PaymentDto

    {

        public int UserId { get; set; }

        public int ProductId { get; set; }

        public string MethodName { get; set; } // "Credit", "Debit", "UPI", etc.

        public int TenureMonths { get; set; }  // EMI months; if 1, no EMI applied

    }

    // For Card/EMI payments

    public class CardPaymentDto

    {

        public int UserId { get; set; }

        public int ProductId { get; set; }

        public int EmiCardId { get; set; }    // EMI Card being used

        public int TenureMonths { get; set; } // EMI tenure; even with 0% interest, it's used to track installments

    }

    // For Razorpay payment verification

    public class VerifyPaymentDto

    {

        public string RazorpayOrderId { get; set; }

        public string RazorpayPaymentId { get; set; }

        public string RazorpaySignature { get; set; }

        public int UserId { get; set; }

        public int ProductId { get; set; }

        public decimal Amount { get; set; } // Optional: you can use it for cross-checking if required
        public int InstallmentId { get; set; }
    }

    // For Razorpay order creation

    public class OrderRequestDto

    {

        public int ProductId { get; set; }

        public int UserId { get; set; }

    }

}