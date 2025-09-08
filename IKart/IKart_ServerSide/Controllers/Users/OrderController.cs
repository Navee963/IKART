using IKart_ServerSide.Models;
using IKart_Shared.DTOs.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IKart_ServerSide.Controllers.Users
{
    [RoutePrefix("api/orders")]
    public class OrderController : ApiController
    {
        private readonly IKartEntities db = new IKartEntities();

        // ✅ Place Order
        [HttpPost]
        [Route("place")]
        public IHttpActionResult PlaceOrder(COD_UPI_OrdersDto dto)
        {
            try
            {
                // Basic validation
                if (dto.ProductId <= 0 || dto.UserId <= 0 || string.IsNullOrWhiteSpace(dto.PaymentType))
                    return BadRequest("Invalid order data.");

                var paymentType = dto.PaymentType.Trim().ToUpper();

                if (paymentType != "COD" && paymentType != "UPI")
                    return BadRequest("Unsupported payment type. Use 'COD' or 'UPI'.");

                var order = new COD_UPI_Orders
                {
                    ProductId = dto.ProductId,
                    UserId = dto.UserId,
                    PaymentType = paymentType,
                    PaymentStatus = paymentType == "COD" ? "Pending" : "Paid",
                    OrderDate = dto.OrderDate != default ? dto.OrderDate : DateTime.Now,
                    DeliveryDate = dto.DeliveryDate != default ? dto.DeliveryDate : DateTime.Now.AddDays(5)
                };

                db.COD_UPI_Orders.Add(order);
                db.SaveChanges();

                return Ok(new { Message = "Order Confirmed", OrderId = order.OrderId });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error placing order: " + ex.Message));
            }
        }


        // ✅ Get all orders for a user
        [HttpGet]
        [Route("user/{userId}")]
        public IHttpActionResult GetOrdersByUser(int userId)
        {
            var orders = db.Orders
                .Where(o => o.UserId == userId)
                .Select(o => new OrderDto
                {
                    Order_Id = o.Order_Id,
                    ProductId = o.ProductId ?? 0,
                    UserId = o.UserId ?? 0,
                    PaymentId = o.PaymentId ?? 0,
                    OrderDate = o.OrderDate ?? DateTime.MinValue,
                    DeliveryDate = o.DeliveryDate ?? DateTime.MinValue
                }).ToList();

            return Ok(orders);
        }

        // ✅ Get order by ID
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetOrder(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
                return NotFound();

            var dto = new OrderDto
            {
                Order_Id = order.Order_Id,
                ProductId = order.ProductId ?? 0,
                UserId = order.UserId ?? 0,
                PaymentId = order.PaymentId ?? 0,
                OrderDate = order.OrderDate ?? DateTime.MinValue,
                DeliveryDate = order.DeliveryDate ?? DateTime.MinValue
            };

            return Ok(dto);
        }
    }
}
