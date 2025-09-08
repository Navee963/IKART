using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IKart_ServerSide.Models
{
    public class ProductDto
    {
        public string ProductName { get; set; }
        public decimal Cost { get; set; }
        public string ProductDetails { get; set; }
        public string ProductImage { get; set; }
        public int Stock_Id { get; set; }
    }
}