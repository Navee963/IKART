using IKart_ServerSide.Models;
using IKart_Shared.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace IKart_ServerSide.Controllers.Admin
{
    [RoutePrefix("api/emicards")]
    public class EmiCardController : ApiController
    {
        private readonly IKartEntities db = new IKartEntities();

        // GET: api/emicards
        [HttpGet, Route("")]
        public IHttpActionResult GetAll()
        {
            var cards = (from cr in db.Card_Request
                         join u in db.Users on cr.UserId equals u.UserId
                         join jf in db.Joining_Fee on cr.Card_Id equals jf.Card_Id into jfGroup
                         from joiningFee in jfGroup.DefaultIfEmpty()
                         select new EmiCardDto
                         {
                             CardId = cr.Card_Id,
                             UserId = u.UserId,
                             UserName = u.FullName,
                             Email = u.Email,
                             ApprovalStatus = (cr.IsVerified == true) ? "Approved" : (cr.IsVerified == false ? "Rejected" : "Pending"),
                             FeeAmount = (decimal)(joiningFee != null ? joiningFee.Amount : 0),
                             FeeStatus = joiningFee != null ? joiningFee.Status : "Not Available",
                             Documents = db.EmiCard_Documents
                                           .Where(d => d.Card_Id == cr.Card_Id)
                                           .Select(d => new EmiCardDocumentDto
                                           {
                                               DocumentId = d.DocumentId,
                                               CardId = d.Card_Id ?? 0,
                                               DocumentType = d.DocumentType,
                                               FileName = d.FileName,
                                               FilePath = d.FilePath
                                           }).ToList()
                         }).ToList();

            return Ok(cards);
        }

        // GET: api/emicards/{id}
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            var card = (from cr in db.Card_Request
                        join u in db.Users on cr.UserId equals u.UserId
                        join jf in db.Joining_Fee on cr.Card_Id equals jf.Card_Id into jfGroup
                        from joiningFee in jfGroup.DefaultIfEmpty()
                        where cr.Card_Id == id
                        select new EmiCardDto
                        {
                            CardId = cr.Card_Id,
                            UserId = u.UserId,
                            UserName = u.FullName,
                            Email = u.Email,
                            ApprovalStatus = (cr.IsVerified == true) ? "Approved" : (cr.IsVerified == false ? "Rejected" : "Pending"),
                            FeeAmount = (decimal)(joiningFee != null ? joiningFee.Amount : 0),
                            FeeStatus = joiningFee != null ? joiningFee.Status : "Not Available",
                            Documents = db.EmiCard_Documents
                                           .Where(d => d.Card_Id == cr.Card_Id)
                                           .Select(d => new EmiCardDocumentDto
                                           {
                                               DocumentId = d.DocumentId,
                                               CardId = d.Card_Id ?? 0,
                                               DocumentType = d.DocumentType,
                                               FileName = d.FileName,
                                               FilePath = d.FilePath
                                           }).ToList()
                        }).FirstOrDefault();

            if (card == null)
                return NotFound();

            return Ok(card);
        }

        // PUT: api/emicards/updatestatus/{id}
        [HttpPut, Route("updatestatus/{id:int}")]
        public IHttpActionResult UpdateStatus(int id, [FromBody] string status)
        {
            var cardReq = db.Card_Request.FirstOrDefault(c => c.Card_Id == id);
            if (cardReq == null) return NotFound();

            if (status == "Approved")
            {
                cardReq.IsVerified = true;

                // Check if EMI Card already exists for this user and card type
                bool alreadyExists = db.EMI_Card.Any(e => e.UserId == cardReq.UserId && e.CardType == cardReq.CardType);

                if (!alreadyExists)
                {
                    decimal totalLimit = cardReq.CardType == "Gold" ? 25000 :
                                        cardReq.CardType == "Diamond" ? 50000 : 100000;

                    string cardNumber = Guid.NewGuid().ToString("N").Substring(0, 16);
                    string userName = db.Users.FirstOrDefault(u => u.UserId == cardReq.UserId)?.FullName ?? "";

                    // Generate the virtual card image and get the relative path
                    string cardImagePath = GenerateCardImage(cardReq.CardType, cardNumber, userName);

                    var emiCard = new EMI_Card
                    {
                        UserId = cardReq.UserId,
                        CardType = cardReq.CardType,
                        CardNumber = cardNumber,
                        TotalLimit = totalLimit,
                        Balance = totalLimit,
                        IsActive = true,
                        IssueDate = DateTime.Now,
                        ExpireDate = DateTime.Now.AddYears(3),
                        CardImage = cardImagePath // <-- Store the image path in DB
                    };
                    db.EMI_Card.Add(emiCard);
                }
            }
            else if (status == "Rejected")
            {
                cardReq.IsVerified = false;

                // (Optional) Refund logic can go here, if you integrate Razorpay refunds
            }

            db.SaveChanges();
            return Ok("Approval status updated");
        }

        /// <summary>
        /// Generates a PNG image for the virtual card, returns relative path for storage in CardImage column
        /// </summary>
        private string GenerateCardImage(string cardType, string cardNumber, string userName)
        {
            int width = 400;
            int height = 220;
            var bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.FromArgb(0, 60, 150));
                using (var fontType = new Font("Arial", 16, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                    g.DrawString(cardType + " Card", fontType, brush, 20, 20);

                using (var fontNum = new Font("Courier New", 20, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                    g.DrawString(cardNumber, fontNum, brush, 20, 80);

                using (var fontName = new Font("Arial", 12, FontStyle.Regular))
                using (var brush = new SolidBrush(Color.White))
                    g.DrawString(userName, fontName, brush, 20, 150);

                using (var fontBrand = new Font("Arial", 10, FontStyle.Italic))
                using (var brush = new SolidBrush(Color.LightGray))
                    g.DrawString("IKart Virtual", fontBrand, brush, width - 120, height - 30);
            }
            string fileName = Guid.NewGuid().ToString() + ".png";
            string relPath = "/Content/EmiCardImages/" + fileName;
            string serverPath = System.Web.Hosting.HostingEnvironment.MapPath(relPath);
            Directory.CreateDirectory(Path.GetDirectoryName(serverPath));
            bitmap.Save(serverPath, ImageFormat.Png);
            return relPath;
        }
    }
}