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
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private readonly IKartEntities db = new IKartEntities();

        // GET api/orders
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetOrders()
        {
            var orders = db.Orders
                .Include("User")
                .Include("Payment")
                .Include("Product")
                .Select(o => new OrderDto
                {
                    Order_Id = o.Order_Id,
                    UserName = o.User.FullName,
                    Status = o.Payment.Status,
                    TotalAmount = o.Payment.TotalAmount ?? 0,    // FIX
                    OrderDate = o.OrderDate ?? DateTime.MinValue // FIX
                }).ToList();

            return Ok(orders);
        }

        // GET api/orders/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetOrderDetails(int id)
        {
            var order = db.Orders
                .Include("User")
                .Include("Product")
                .Include("Payment.Monthly_EMI_Calc.Installment_Payments")
                .Include("Payment.Payment_Methods")
                .FirstOrDefault(o => o.Order_Id == id);

            if (order == null)
                return NotFound();

            var emi = order.Payment.Monthly_EMI_Calc.FirstOrDefault();

            var details = new OrderDetailsDto
            {
                Order_Id = order.Order_Id,
                UserName = order.User.FullName,
                ProductName = order.Product.ProductName,
                ProductCost = order.Product.Cost ?? 0,  // FIX
                PaymentType = order.Payment.Payment_Methods.MethodName,
                PaidAmount = emi != null
                                ? (emi.TotalAmount ?? 0) - (emi.RemainingAmount ?? 0)
                                : (order.Payment.TotalAmount ?? 0), // FIX
                RemainingAmount = emi?.RemainingAmount ?? 0, // FIX
                TenureMonths = emi?.TenureMonths ?? 0,       // FIX
                MonthsRemaining = emi != null
                                    ? (emi.TenureMonths ?? 0) - emi.Installment_Payments.Count(i => (i.IsPaid ?? false))
                                    : 0, // FIX
                OrderDate = order.OrderDate ?? DateTime.MinValue,   // FIX
                DeliveryDate = order.DeliveryDate, // already nullable, keep as is
                Payments = order.Payment.Monthly_EMI_Calc
                    .SelectMany(m => m.Installment_Payments)
                    .Select(i => new PaymentHistoryDto
                    {
                        PaymentDate = i.DueDate ?? DateTime.MinValue, // FIX
                        Amount = i.Amount ?? 0,                       // FIX
                        Status = (i.IsPaid ?? false) ? "Paid" : "Pending"
                    }).ToList()
            };

            return Ok(details);
        }
    }
}