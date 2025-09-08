using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using IKart_ServerSide.Models;
using IKart_Shared.DTOs.EMI_Card;

namespace IKart_ServerSide.Controllers.Users
{
    [RoutePrefix("api/emicards")]
    public class EMICardsController : ApiController
    {
        private readonly IKartEntities db = new IKartEntities();

        // 1️⃣ Request EMI Card
        [HttpPost]
        [Route("request")]
        public IHttpActionResult RequestCard(CardRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

            if (!db.Users.Any(u => u.UserId == dto.UserId))
                return BadRequest("User does not exist");

            if (db.Card_Request.Any(r => r.UserId == dto.UserId && r.IsVerified == false))
                return BadRequest("You already have a pending request");

            var request = new Card_Request
            {
                UserId = dto.UserId,
                CardType = dto.CardType,
                BankName = dto.BankName.Trim(),
                AccountNumber = dto.AccountNumber.Trim(),
                IFSC_Code = dto.IFSC_Code.Trim(),
                AadhaarNumber = dto.AadhaarNumber.Trim(),
                IsVerified = false
            };

            db.Card_Request.Add(request);
            db.SaveChanges();

            dto.Card_Id = request.Card_Id;

            // Determine joining fee based on card type (FIXED: Gold = 1000)
            decimal fee = 3000; // Default fee
            if (dto.CardType == "Gold")
                fee = 1000;
            else if (dto.CardType == "Diamond")
                fee = 2000;

            var joiningFee = new Joining_Fee
            {
                Card_Id = request.Card_Id,
                PaymentMethodId = null,
                Amount = fee,
                Status = "Pending"
            };

            db.Joining_Fee.Add(joiningFee);
            db.SaveChanges();

            return Ok(new { message = "Card request submitted successfully", dto });
        }

        // 2️⃣ Upload Documents for EMI Card
        [HttpPost]
        [Route("upload-documents/{cardId}")]
        public async Task<IHttpActionResult> UploadDocuments(int cardId)
        {
            var request = db.Card_Request.Find(cardId);
            if (request == null)
                return BadRequest("Invalid Card Request ID");

            if (!HttpContext.Current.Request.Files.AllKeys.Any())
                return BadRequest("No files received");

            var allowedTypes = new[] { "Aadhaar", "PAN", "BankBook" };
            var uploadedDocs = new List<EmiCard_Documents>();

            foreach (string key in HttpContext.Current.Request.Files)
            {
                var file = HttpContext.Current.Request.Files[key];
                if (file == null || file.ContentLength == 0)
                    continue;

                if (!allowedTypes.Contains(key))
                    continue;

                var fileName = Path.GetFileName(file.FileName);
                var serverPath = HttpContext.Current.Server.MapPath("~/Content/EmiCardDocuments/");
                Directory.CreateDirectory(serverPath);
                var fullPath = Path.Combine(serverPath, fileName);
                file.SaveAs(fullPath);

                var doc = new EmiCard_Documents
                {
                    Card_Id = cardId,
                    DocumentType = key,
                    FileName = fileName,
                    FilePath = "/Content/EmiCardDocuments/" + fileName,
                    UploadedDate = DateTime.Now
                };

                db.EmiCard_Documents.Add(doc);
                uploadedDocs.Add(doc);
            }

            await db.SaveChangesAsync();

            return Ok(new { message = "Documents uploaded successfully", documents = uploadedDocs });
        }

        // 3️⃣ Pay Joining Fee
        [HttpPost]
        [Route("payfee/{cardId}")]
        public IHttpActionResult PayJoiningFee(int cardId, [FromBody] PaymentDto payment)
        {
            if (payment == null)
                return BadRequest("Payment data is required.");

            var method = db.Payment_Methods.FirstOrDefault(m => m.PaymentMethodId == payment.PaymentMethodId);
            if (method == null)
                return BadRequest("Invalid Payment Method");

            var joiningFee = db.Joining_Fee.FirstOrDefault(j => j.Card_Id == cardId);
            if (joiningFee == null)
                return NotFound();

            joiningFee.Status = "Paid";
            joiningFee.PaymentMethodId = payment.PaymentMethodId;
            joiningFee.Amount = payment.Amount;

            db.SaveChanges();

            return Ok(new { message = "Payment successful. Await admin approval." });
        }

        // 4️⃣ Get EMI Cards for a user (with CardImage property)
        [HttpGet]
        [Route("user/{userId}")]
        public IHttpActionResult GetUserEmiCards(int userId)
        {
            // Materialize first, THEN use Path.GetFileName (LINQ-to-Objects)
            var rows = db.EMI_Card
                .Where(ec => ec.UserId == userId)
                .ToList();

            var emiCards = rows.Select(ec => new IKart_Shared.DTOs.EMI_Card.EmiCardDto
            {
                EmiCardId = ec.EmiCardId,
                CardId = ec.EmiCardId,
                UserId = (int)ec.UserId,
                CardType = ec.CardType,
                CardNumber = ec.CardNumber,
                TotalLimit = (decimal)ec.TotalLimit,
                Balance = (decimal)ec.Balance,
                IsActive = (bool)ec.IsActive,
                IssueDate = ec.IssueDate,
                ExpireDate = ec.ExpireDate,

                // Only the filename (e.g., "accb63338c8145cd.png")
                CardImage = string.IsNullOrWhiteSpace(ec.CardImage)
                    ? null
                    : Path.GetFileName(ec.CardImage)
            }).ToList();

            return Ok(emiCards);
        }

        // 5️⃣ Serve EMI Card Images by filename
        //[HttpGet]
        //[Route("image/{fileName}")]
        //public IHttpActionResult GetCardImage(string fileName)
        //{
        //    if (string.IsNullOrWhiteSpace(fileName))
        //        return BadRequest("Missing file name.");

        //    var path = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/EmiCardImages/" + fileName);
        //    if (!System.IO.File.Exists(path))
        //        return NotFound();

        //    var bytes = System.IO.File.ReadAllBytes(path);
        //    var result = new HttpResponseMessage(HttpStatusCode.OK)
        //    {
        //        Content = new ByteArrayContent(bytes)
        //    };
        //    result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        //    return ResponseMessage(result);
        //}

        [HttpGet]
        [Route("image/{fileName}")]
        public IHttpActionResult GetCardImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("Missing file name.");

            var path = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/EmiCardImages/" + fileName);
            if (!System.IO.File.Exists(path))
                return NotFound();

            var bytes = System.IO.File.ReadAllBytes(path);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return ResponseMessage(result);
        }


    }
}