using IKart_ServerSide.Models;
using IKart_Shared.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IKart_ServerSide.Controllers.Admin
{
    [RoutePrefix("api/dashboard")]

    public class DashboardController : ApiController
    {
        private readonly IKartEntities db = new IKartEntities();

        // ✅ Get Tokens Raised
        [HttpGet]
        [Route("tokens")]
        public IHttpActionResult GetTokens()
        {
            var tokens = db.Support_Tickets
                           .OrderByDescending(t => t.CreatedDate)
                           .Take(5)
                           .Select(t => new
                           {
                               t.TicketId,
                               t.Subject,
                               t.Status,
                               t.CreatedDate
                           })
                           .ToList();
            return Ok(tokens);
        }

        // ✅ Get Recent Orders (OrderDto)
        [HttpGet]
        [Route("recent-orders")]
        public IHttpActionResult GetRecentOrders()
        {
            var orders = db.Orders
                           .OrderByDescending(o => o.OrderDate)
                           .Take(5)
                           .Select(o => new OrderDto
                           {
                               Order_Id = o.Order_Id,
                               UserName = o.User.FullName,
                               Status = o.Payment.Status,
                               TotalAmount = (decimal)o.Payment.TotalAmount,
                               OrderDate = (DateTime)o.OrderDate
                           })
                           .ToList();

            return Ok(orders);
        }

        // ✅ Revenue (Last 7 days)
        [HttpGet]
        [Route("revenue")]
        public IHttpActionResult GetRevenue()
        {
            var today = DateTime.Now.Date;

            // Pull payments first into memory
            var payments = db.Payments
                             .Where(p => p.Status == "Paid")
                             .Select(p => new { p.PaymentDate, p.TotalAmount })
                             .ToList(); // ✅ Materialize into memory

            var revenue = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var date = today.AddDays(-i);
                    var total = payments
                        .Where(p => p.PaymentDate >= date && p.PaymentDate < date.AddDays(1))
                        .Sum(p => (decimal?)p.TotalAmount) ?? 0;

                    return new
                    {
                        Date = date,
                        Total = total
                    };
                })
                .OrderBy(r => r.Date)
                .ToList();

            return Ok(revenue);
        }

    }
}
