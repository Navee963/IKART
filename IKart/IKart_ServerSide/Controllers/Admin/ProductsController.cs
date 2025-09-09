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

        // Helper for stock update (NOT USED for add/delete product anymore!)
        private void ChangeStockAvailable(int? stockId, int delta)
        {
            if (stockId == null) return;
            var stock = db.Stocks.Find(stockId);
            if (stock != null)
            {
                stock.Available_Stocks = (stock.Available_Stocks ?? 0) + delta;
                db.SaveChanges();
            }
        }

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

        // Add new product (NO stock change)
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

            // DO NOT change AvailableStocks here

            dto.ProductId = p.ProductId;
            dto.CreatedDate = p.CreatedDate;

            // Optionally fetch category info after insert
            var stock = db.Stocks.Find(p.Stock_Id);
            dto.Category = stock?.CategoryName;
            dto.SubCategory = stock?.SubCategoryName;

            return Ok(dto);
        }

        // Update product (NO stock change)
        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult Update(int id, ProductDto dto)
        {
            var p = db.Products.Find(id);
            if (p == null) return NotFound();

            var oldStockId = p.Stock_Id;
            var newStockId = dto.Stock_Id;

            p.ProductName = dto.ProductName;
            p.Cost = dto.Cost;
            p.ProductDetails = dto.ProductDetails;
            p.ProductImage = dto.ProductImage;
            p.Stock_Id = dto.Stock_Id;

            db.SaveChanges();

            // DO NOT change AvailableStocks here

            // Optionally update category info
            var stock = db.Stocks.Find(p.Stock_Id);
            dto.Category = stock?.CategoryName;
            dto.SubCategory = stock?.SubCategoryName;

            return Ok(dto);
        }

        // Delete product (NO stock change)
        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult Delete(int id)
        {
            var p = db.Products.Find(id);
            if (p == null) return NotFound();

            // var stockId = p.Stock_Id;

            db.Products.Remove(p);
            db.SaveChanges();

            // DO NOT change AvailableStocks here

            return Ok("Deleted");
        }
    }
}