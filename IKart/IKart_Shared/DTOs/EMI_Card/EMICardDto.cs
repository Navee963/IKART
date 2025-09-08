using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IKart_Shared.DTOs.EMI_Card
{
    public class EmiCardDto
    {
        public int EmiCardId { get; set; }

        [Required]
        public int CardId { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public decimal TotalLimit { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public decimal FeePaid { get; set; }
        public string Status { get; set; }   // Pending / Approved / Rejected
        public string CardImage { get; set; }
        public List<EmiCardDocumentDto> Documents { get; set; }
    }

    public class EmiCardDocumentDto
    {
        public int DocumentId { get; set; }
        public int CardId { get; set; }
        public string DocumentType { get; set; } // Aadhaar, AddressProof, FeeReceipt
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}