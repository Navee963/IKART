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
    [RoutePrefix("api/refunds")]
    public class RefundsController : ApiController
    {
        private readonly IKartEntities db = new IKartEntities();

        // GET: api/refunds
        [HttpGet]
        [Route("")]
        public List<RefundDto> GetAllRefunds()
        {
            var refunds = (from r in db.Refunds
                           join p in db.Payments on r.PaymentId equals p.PaymentId
                           join u in db.Users on p.UserId equals u.UserId
                           join prod in db.Products on p.ProductId equals prod.ProductId
                           select new RefundDto
                           {
                               RefundId = r.RefundId,
                               PaymentId = p.PaymentId,
                               Amount = r.Amount ?? 0m,                  // handle nullable decimal
                               Reason = r.Reason,
                               Status = r.Status,
                               RefundDate = r.RefundDate ?? DateTime.MinValue, // handle nullable DateTime
                               UserId = u.UserId,
                               UserName = u.FullName,
                               UserEmail = u.Email,
                               ProductId = prod.ProductId,
                               ProductName = prod.ProductName,
                               TotalAmount = p.TotalAmount ?? 0m,       // handle nullable decimal
                               PaymentStatus = p.Status
                           }).ToList();

            return refunds;
        }

        // GET: api/refunds/5
        [HttpGet]
        [Route("{id}")]
        public RefundDto GetRefundById(int id)
        {
            var refund = (from r in db.Refunds
                          join p in db.Payments on r.PaymentId equals p.PaymentId
                          join u in db.Users on p.UserId equals u.UserId
                          join prod in db.Products on p.ProductId equals prod.ProductId
                          where r.RefundId == id
                          select new RefundDto
                          {
                              RefundId = r.RefundId,
                              PaymentId = p.PaymentId,
                              Amount = r.Amount ?? 0m,
                              Reason = r.Reason,
                              Status = r.Status,
                              RefundDate = r.RefundDate ?? DateTime.MinValue,
                              UserId = u.UserId,
                              UserName = u.FullName,
                              UserEmail = u.Email,
                              ProductId = prod.ProductId,
                              ProductName = prod.ProductName,
                              TotalAmount = p.TotalAmount ?? 0m,
                              PaymentStatus = p.Status
                          }).FirstOrDefault();

            return refund;
        }
    }
}
