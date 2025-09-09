using System;
using System.Linq;
using System.Web.Http;
using IKart_ServerSide.Models;
using IKart_Shared.DTOs;

namespace IKart_ServerSide.Controllers.Users
{
    [RoutePrefix("api/product")]
    public class ProductController : ApiController
    {
        IKartEntities db = new IKartEntities();

        [HttpGet]
        [Route("all")]
        public IHttpActionResult GetAllProducts()
        {
            var products = db.Products
                .Where(p => p.Stock != null && p.Stock.Available_Stocks > 0) // ✅ Only products with stock
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Cost = (decimal)p.Cost,
                    ProductImage = p.ProductImage,
                    CreatedDate = (DateTime)p.CreatedDate,
                    AvailableQuantity = p.Stock.Available_Stocks ?? 0,  // ✅ from Stocks
                    CategoryName = p.Stock.CategoryName,                   // ✅ from Stocks
                    SubCategoryName = p.Stock.SubCategoryName              // ✅ from Stocks
                })
                .ToList();

            return Ok(products);
        }

        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetProduct(int id)
        {
            var p = db.Products
                .FirstOrDefault(x => x.ProductId == id && x.Stock != null && x.Stock.Available_Stocks > 0);

            if (p == null) return NotFound();

            var dto = new ProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Cost = (decimal)p.Cost,
                ProductDetails = p.ProductDetails,
                ProductImage = p.ProductImage,
                CreatedDate = (DateTime)p.CreatedDate,
                AvailableQuantity = p.Stock.Available_Stocks ?? 0,  // ✅ from Stocks
                CategoryName = p.Stock.CategoryName,                   // ✅ from Stocks
                SubCategoryName = p.Stock.SubCategoryName              // ✅ from Stocks
            };

            return Ok(dto);
        }
    }
}