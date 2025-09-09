using System;
using System.Linq;
using System.Web.Http;
using System.Data.Entity;
using IKart_ServerSide.Models;
using IKart_Shared.DTOs;

namespace IKart_ServerSide.Controllers
{
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        IKartEntities db = new IKartEntities();

        // Get all products
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            var data = db.Products
                .Include(p => p.Stock)
                .ToList()
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Cost = (decimal)p.Cost,
                    ProductDetails = p.ProductDetails,
                    ProductImage = p.ProductImage,
                    Stock_Id = (int)p.Stock_Id,
                    CreatedDate = (DateTime)p.CreatedDate,
                    AvailableQuantity = p.Stock?.Available_Stocks ?? 0, // ✅ include stock availability
                    CategoryName = p.Stock?.CategoryName,
                    SubCategoryName = p.Stock?.SubCategoryName
                })
                .ToList();

            return Ok(data);
        }

        // Get single product
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetOne(int id)
        {
            var p = db.Products
                .Include(x => x.Stock)
                .FirstOrDefault(x => x.ProductId == id);

            if (p == null) return NotFound();

            var dto = new ProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Cost = (decimal)p.Cost,
                ProductDetails = p.ProductDetails,
                ProductImage = p.ProductImage,
                Stock_Id = (int)p.Stock_Id,
                CreatedDate = (DateTime)p.CreatedDate,
                AvailableQuantity = p.Stock?.Available_Stocks ?? 0,
                CategoryName = p.Stock?.CategoryName,
                SubCategoryName = p.Stock?.SubCategoryName
            };

            return Ok(dto);
        }

        // Add new product
        [HttpPost]
        [Route("")]
        public IHttpActionResult Add(ProductDto dto)
        {
            // ✅ validate stock
            var stock = db.Stocks.Find(dto.Stock_Id);
            if (stock == null) return BadRequest("Invalid stock ID.");

            var p = new Product
            {
                ProductName = dto.ProductName,
                Cost = dto.Cost,
                ProductDetails = dto.ProductDetails,
                ProductImage = dto.ProductImage,
                Stock_Id = dto.Stock_Id,
                CreatedDate = DateTime.Now
            };

            db.Products.Add(p);
            db.SaveChanges();

            dto.ProductId = p.ProductId;
            dto.CreatedDate = (DateTime)p.CreatedDate;
            dto.AvailableQuantity = stock.Available_Stocks ?? 0;
            dto.CategoryName = stock.CategoryName;
            dto.SubCategoryName = stock.SubCategoryName;

            return Ok(dto);
        }

        // Update product
        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult Update(int id, ProductDto dto)
        {
            var p = db.Products.Find(id);
            if (p == null) return NotFound();

            // ✅ validate stock
            var stock = db.Stocks.Find(dto.Stock_Id);
            if (stock == null) return BadRequest("Invalid stock ID.");

            p.ProductName = dto.ProductName;
            p.Cost = dto.Cost;
            p.ProductDetails = dto.ProductDetails;
            p.ProductImage = dto.ProductImage;
            p.Stock_Id = dto.Stock_Id;

            db.SaveChanges();

            dto.AvailableQuantity = stock.Available_Stocks ?? 0;
            dto.CategoryName = stock.CategoryName;
            dto.SubCategoryName = stock.SubCategoryName;

            return Ok(dto);
        }

        // Delete product
        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult Delete(int id)
        {
            var p = db.Products.Find(id);
            if (p == null) return NotFound();

            db.Products.Remove(p);
            db.SaveChanges();

            return Ok("Deleted");
        }
    }
}