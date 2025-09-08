using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using IKart_ServerSide.Models;
using IKart_Shared.DTOs;

namespace IKart_ServerSide.Controllers
{
    [RoutePrefix("api/stocks")]
    public class StocksController : ApiController
    {
        IKartEntities db = new IKartEntities();

        // =========================
        // GET ALL STOCKS
        // =========================
        [HttpGet, Route("")]
        public IHttpActionResult GetStocks()
        {
            var stocks = db.Stocks
                .Select(s => new StocksDto
                {
                    StockId = s.Stock_Id,
                    Category = s.CategoryName,
                    SubCategory = s.SubCategoryName,
                    TotalStocks = s.Total_Stocks,
                    AvailableStocks = s.Available_Stocks
                })
                .ToList();

            return Ok(stocks);
        }

        // =========================
        // GET SINGLE STOCK BY ID
        // =========================
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetStock(int id)
        {
            var stock = db.Stocks.Where(s => s.Stock_Id == id)
                .Select(s => new StocksDto
                {
                    StockId = s.Stock_Id,
                    Category = s.CategoryName,
                    SubCategory = s.SubCategoryName,
                    TotalStocks = s.Total_Stocks,
                    AvailableStocks = s.Available_Stocks
                })
                .FirstOrDefault();

            if (stock == null) return NotFound();
            return Ok(stock);
        }

        // =========================
        // CREATE STOCK
        // =========================
        [HttpPost, Route("")]
        public IHttpActionResult CreateStock(StocksDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var stock = new Stock
            {
                CategoryName = dto.Category,
                SubCategoryName = dto.SubCategory,
                Total_Stocks = dto.TotalStocks,
                Available_Stocks = dto.AvailableStocks ?? dto.TotalStocks
            };

            db.Stocks.Add(stock);
            db.SaveChanges();

            dto.StockId = stock.Stock_Id;
            return Ok(dto);
        }

        // =========================
        // UPDATE STOCK
        // =========================
        [HttpPut, Route("{id:int}")]
        public IHttpActionResult UpdateStock(int id, StocksDto dto)
        {
            var stock = db.Stocks.Find(id);
            if (stock == null) return NotFound();

            stock.CategoryName = dto.Category;
            stock.SubCategoryName = dto.SubCategory;
            stock.Total_Stocks = dto.TotalStocks;
            stock.Available_Stocks = dto.AvailableStocks;

            db.SaveChanges();
            return Ok("Stock updated successfully");
        }

        // =========================
        // DELETE STOCK
        // =========================
        // =========================
        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult DeleteStock(int id)
        {
            var stock = db.Stocks.Find(id);
            if (stock == null) return NotFound();

            // Check if this stock is used by any products
            bool isUsed = db.Products.Any(p => p.Stock_Id == id);
            if (isUsed)
            {
                return Content(HttpStatusCode.Conflict, "Cannot delete this stock because it is used by products.");
            }

            db.Stocks.Remove(stock);
            db.SaveChanges();
            return Ok("Stock deleted successfully");
        }

    }
}
