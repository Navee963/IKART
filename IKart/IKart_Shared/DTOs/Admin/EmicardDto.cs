using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs.Admin
{
    public class EmiCardDto
    {
        public int CardId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public decimal FeeAmount { get; set; }
        public string FeeStatus { get; set; }    // ✅ Fee Paid Status (from Joining_Fee.Status)
        public string ApprovalStatus { get; set; }  // ✅ Approved/Rejected/Pending (from Card_Request.IsVerified)

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
