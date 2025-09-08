using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IKart_Shared.DTOs
{
    public class AddressDto
    {
        public int AddressId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string Street { get; set; }

        [Required, StringLength(50)]
        public string City { get; set; }

        [Required, StringLength(50)]
        public string State { get; set; }

        [Required, StringLength(10)]
        public string ZipCode { get; set; }

        [Required, StringLength(50)]
        public string Country { get; set; }
    }

}