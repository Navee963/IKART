using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
                .Include(p => p.Stock) // Include related Stock data
                .ToList()
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Cost = p.Cost,
                    ProductDetails = p.ProductDetails,
                    ProductImage = p.ProductImage,
                    Stock_Id = p.Stock_Id,
                    CreatedDate = p.CreatedDate,
                    Category = p.Stock?.CategoryName,
                    SubCategory = p.Stock?.SubCategoryName
                }).ToList();

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
                Cost = p.Cost,
                ProductDetails = p.ProductDetails,
                ProductImage = p.ProductImage,
                Stock_Id = p.Stock_Id,
                CreatedDate = p.CreatedDate,
                Category = p.Stock?.CategoryName,
                SubCategory = p.Stock?.SubCategoryName
            };

            return Ok(dto);
        }

        // Add new product
        [HttpPost]
        [Route("")]
        public IHttpActionResult Add(ProductDto dto)
        {
            Product p = new Product
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
            dto.CreatedDate = p.CreatedDate;

            // Optionally fetch category info after insert
            var stock = db.Stocks.Find(p.Stock_Id);
            dto.Category = stock?.CategoryName;
            dto.SubCategory = stock?.SubCategoryName;

            return Ok(dto);
        }

        // Update product
        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult Update(int id, ProductDto dto)
        {
            var p = db.Products.Find(id);
            if (p == null) return NotFound();

            p.ProductName = dto.ProductName;
            p.Cost = dto.Cost;
            p.ProductDetails = dto.ProductDetails;
            p.ProductImage = dto.ProductImage;
            p.Stock_Id = dto.Stock_Id;

            db.SaveChanges();

            // Optionally update category info
            var stock = db.Stocks.Find(p.Stock_Id);
            dto.Category = stock?.CategoryName;
            dto.SubCategory = stock?.SubCategoryName;

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
