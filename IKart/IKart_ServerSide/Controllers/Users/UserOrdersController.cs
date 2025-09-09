using System;

using System.Collections.Generic;

using System.Linq;

using System.Web.Http;

using IKart_Shared.DTOs.Orders;

using IKart_ServerSide.Models; // Your EF context namespace

namespace IKart_ServerSide.Controllers

{

    [RoutePrefix("api/userorders")]

    public class UserOrdersController : ApiController

    {

        private readonly IKartEntities db = new IKartEntities();

        [HttpGet]

        [Route("all/{userId}")]

        public IHttpActionResult GetUserOrders(int userId)

        {

            try

            {

                var onlineOrders = db.Orders

                    .Where(o => o.UserId == userId)

                    .Select(o => new UserOrderDto

                    {

                        OrderId = o.Order_Id,

                        ProductName = o.Product.ProductName,

                        PaymentType = "Online",

                        PaymentStatus = o.Payment.Status,

                        OrderDate = (DateTime)o.OrderDate,

                        DeliveryDate = (DateTime)o.DeliveryDate

                    });

                var codUpiOrders = db.COD_UPI_Orders

                    .Where(o => o.UserId == userId)

                    .Select(o => new UserOrderDto

                    {

                        OrderId = o.OrderId,

                        ProductName = o.Product.ProductName,

                        PaymentType = o.PaymentType,

                        PaymentStatus = o.PaymentStatus,

                        OrderDate = o.OrderDate,

                        DeliveryDate = o.DeliveryDate

                    });

                var allOrders = onlineOrders

                    .Concat(codUpiOrders)

                    .OrderByDescending(o => o.OrderDate)

                    .ToList();

                return Ok(allOrders);

            }

            catch (Exception ex)

            {

                return InternalServerError(ex);

            }

        }


    }

}

