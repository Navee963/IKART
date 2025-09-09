using System;
using System.Linq;
using System.Web.Http;
using IKart_ServerSide.Models;
using IKart_Shared.DTOs;

namespace IKart_ServerSide.Controllers
{
    [RoutePrefix("api/faq")]
    public class FAQController : ApiController
    {
        private readonly IKartEntities db = new IKartEntities();
        [HttpGet]
        [Route("product/{productId}")]
        public IHttpActionResult GetFAQs(int productId)
        {
            var faqs = db.FAQs
                         .Where(f => f.ProductId == productId)
                         .OrderByDescending(f => f.CreatedDate)
                         .Select(f => new FAQDto
                         {
                             FaqId = f.FaqId,
                             ProductId = (int)f.ProductId,
                             Question = f.Question,
                             Answer = f.Answer,
                             CreatedDate = f.CreatedDate
                         })
                         .ToList();

            return Ok(faqs);
        }


        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetFAQ(int id)
        {
            var faq = db.FAQs.Find(id);
            if (faq == null) return NotFound();
            return Ok(faq);
        }

        [HttpPost]
        [Route("add")]
        public IHttpActionResult AddFAQ(FAQ faq)
        {
            if (faq == null || string.IsNullOrWhiteSpace(faq.Question)) return BadRequest("Invalid FAQ");

            faq.CreatedDate = DateTime.Now;
            db.FAQs.Add(faq);
            db.SaveChanges();
            return Ok(faq);
        }

        [HttpPut]
        [Route("update/{id}")]
        public IHttpActionResult UpdateFAQ(int id, FAQ faq)
        {
            var existing = db.FAQs.Find(id);
            if (existing == null) return NotFound();

            existing.Question = faq.Question;
            existing.Answer = faq.Answer;
            db.SaveChanges();
            return Ok(existing);
        }

        [HttpDelete]
        [Route("delete/{id}")]
        public IHttpActionResult DeleteFAQ(int id)
        {
            var faq = db.FAQs.Find(id);
            if (faq == null) return NotFound();

            db.FAQs.Remove(faq);
            db.SaveChanges();
            return Ok();
        }
    }
}