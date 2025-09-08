using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace IKart_Shared.DTOs.EMI_Card
{
    public class CardRequestDto
    {
        public int Card_Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [RegularExpression("Gold|Diamond|Platinum", ErrorMessage = "Invalid card type")]
        public string CardType { get; set; }

        [Required, StringLength(50)]
        public string BankName { get; set; }

        [Required, RegularExpression(@"^[0-9]{6,18}$", ErrorMessage = "Account number must be 6–18 digits")]
        public string AccountNumber { get; set; }

        [Required, RegularExpression(@"^[A-Za-z]{4}[0-9]{7}$", ErrorMessage = "Invalid IFSC code")]
        public string IFSC_Code { get; set; }

        [Required, RegularExpression(@"^[0-9]{12}$", ErrorMessage = "Aadhaar must be 12 digits")]
        public string AadhaarNumber { get; set; }
        public bool IsVerified { get; set; }
    }
}