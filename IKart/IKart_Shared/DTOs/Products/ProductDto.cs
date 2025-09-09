using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Cost { get; set; }
        public string ProductDetails { get; set; }
        public string ProductImage { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Stock_Id { get; set; }
        public int AvailableQuantity { get; set; }   // ✅ Stock
        public string CategoryName { get; set; }     // ✅ Category
        public string SubCategoryName { get; set; }  // ✅ SubCategory
    }
}