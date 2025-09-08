using IKart_ServerSide.Models;
using IKart_Shared.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IKart_ServerSide.Controllers.Users
{
    [RoutePrefix("api/payments")]
    public class ProductPaymentController : ApiController
    {
        private readonly IKartEntities db = new IKartEntities();

        [HttpGet]
        [Route("user/{userId}")]
        public IHttpActionResult GetPaymentsByUser(int userId)
        {
            var paymentEntities = db.Payments
                .Where(p => p.UserId == userId)
                .ToList();

            var payments = paymentEntities.Select(p =>
            {
                var emi = p.Monthly_EMI_Calc.FirstOrDefault();
                return new UserPaymentDto
                {
                    PaymentId = p.PaymentId,
                    ProductName = p.Product?.ProductName ?? "N/A",
                    CardType = p.EMI_Card?.CardType ?? "N/A",
                    TotalAmount = p.TotalAmount ?? 0m,
                    EMIAmount = emi?.EMIAmount ?? 0m,
                    TenureMonths = emi?.TenureMonths ?? 0,
                    Status = p.Status,
                    PaymentDate = p.PaymentDate ?? DateTime.MinValue
                };
            }).ToList();

            return Ok(payments);
        }

        [HttpGet]
        [Route("details/{paymentId}")]
        public IHttpActionResult GetPaymentDetails(int paymentId)
        {
            var payment = db.Payments.Find(paymentId);
            if (payment == null) return NotFound();

            var emi = payment.Monthly_EMI_Calc.FirstOrDefault();

            var installments = db.Installment_Payments
                .Where(i => i.PaymentId == paymentId) // Use navigation property
                .ToList()
                .Select(i =>
                {
                    var penalty = db.Penalties
                        .FirstOrDefault(p => p.InstallmentId == i.InstallmentId);

                    return new InstallmentDto
                    {
                        InstallmentId = i.InstallmentId,
                        DueDate = i.DueDate ?? DateTime.MinValue,
                        Amount = i.Amount ?? 0m,
                        IsPaid = (bool)i.IsPaid,
                        Penalty = penalty != null
                            ? new PenaltyDto
                            {
                                PenaltyId = penalty.PenaltyId,
                                Days_Overdue = penalty.Days_Overdue ?? 0,
                                PenaltyAmount = penalty.PenaltyAmount ?? 0m,
                                Status = penalty.Status
                            }
                            : null
                    };
                }).ToList();

            var dto = new UserPaymentDto
            {
                PaymentId = payment.PaymentId,
                ProductName = payment.Product?.ProductName ?? "N/A",
                CardType = payment.EMI_Card?.CardType ?? "N/A",
                TotalAmount = payment.TotalAmount ?? 0m,
                EMIAmount = emi?.EMIAmount ?? 0m,
                TenureMonths = emi?.TenureMonths ?? 0,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate ?? DateTime.MinValue,
                Installments = installments
            };

            return Ok(dto);
        }

        [HttpPost]
        [Route("pay-installment/{installmentId}")]
        public IHttpActionResult PayInstallment(int installmentId)
        {
            var installment = db.Installment_Payments.Find(installmentId);
            if (installment == null) return NotFound();

            installment.IsPaid = true;
            db.SaveChanges();

            return Ok();
        }
    }
}
