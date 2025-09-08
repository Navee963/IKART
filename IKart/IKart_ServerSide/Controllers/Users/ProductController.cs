using System;
using System.Linq;
using System.Web.Http;
using System.Data.Entity;
using IKart_ServerSide.Models;
using IKart_Shared.DTOs;

namespace IKart_ServerSide.Controllers.Users
{
    [RoutePrefix("api/product")]
    public class ProductController : ApiController
    {
        IKartEntities db = new IKartEntities();

        // Get all products ordered by latest
        [HttpGet]
        [Route("all")]
        public IHttpActionResult GetAllProducts()
        {
            var products = db.Products
                .OrderByDescending(p => p.CreatedDate)
                .ToList()
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Cost = p.Cost,
                    ProductImage = p.ProductImage,
                    CreatedDate = p.CreatedDate
                }).ToList();

            return Ok(products);
        }

        // Get full details of one product
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetProduct(int id)
        {
            var p = db.Products.Find(id);
            if (p == null) return NotFound();

            var dto = new ProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Cost = p.Cost,
                ProductDetails = p.ProductDetails,
                ProductImage = p.ProductImage,
                CreatedDate = p.CreatedDate
            };

            return Ok(dto);
        }
    }
}