using IKart_ServerSide.Models;

using IKart_Shared.DTOs.Admin;

using System;

using System.Linq;

using System.Web.Http;

namespace IKart_ServerSide.Controllers.Admin

{

    //[RoutePrefix("api/supporttickets")]

    //public class SupportTicketsController : ApiController

    //{

    //    IKartEntities db = new IKartEntities();

    //    // ✅ Get all tickets (Admin)

    //    [HttpGet]

    //    [Route("")]

    //    public IHttpActionResult GetTickets()

    //    {

    //        var tickets = db.Support_Tickets.Select(t => new SupportTicketDto

    //        {

    //            TicketId = t.TicketId,

    //            UserId = (int)t.UserId,

    //            Subject = t.Subject,

    //            Description = t.Description,

    //            Status = t.Status,

    //            CreatedDate = (DateTime)t.CreatedDate,

    //            ClosedDate = t.ClosedDate

    //        }).ToList();

    //        return Ok(tickets);

    //    }

    //    // ✅ User raises a ticket

    //    [HttpPost]

    //    [Route("")]

    //    public IHttpActionResult CreateTicket(SupportTicketDto dto)

    //    {

    //        if (!ModelState.IsValid) return BadRequest(ModelState);

    //        var ticket = new Support_Tickets

    //        {

    //            UserId = dto.UserId,

    //            Subject = dto.Subject,

    //            Description = dto.Description,

    //            Status = "Open",

    //            CreatedDate = DateTime.Now

    //        };

    //        db.Support_Tickets.Add(ticket);

    //        db.SaveChanges();

    //        dto.TicketId = ticket.TicketId;

    //        dto.CreatedDate = (DateTime)ticket.CreatedDate;

    //        dto.Status = "Open";

    //        return Ok(dto);

    //    }

    //}

    [RoutePrefix("api/supporttickets")]

    public class SupportTicketsController : ApiController

    {

        IKartEntities db = new IKartEntities();

        // ✅ Get all tickets (Admin)

        [HttpGet]

        [Route("")]

        public IHttpActionResult GetTickets()

        {

            var tickets = db.Support_Tickets.Select(t => new SupportTicketDto

            {

                TicketId = t.TicketId,

                UserId = (int)t.UserId,

                Username = t.User.Username, // ✅ include username

                Subject = t.Subject,

                Description = t.Description,

                Status = t.Status,

                CreatedDate = (DateTime)t.CreatedDate,

                ClosedDate = t.ClosedDate

            }).ToList();

            return Ok(tickets);

        }

        // ✅ User raises a ticket by username

        [HttpPost]

        [Route("create")]

        public IHttpActionResult CreateTicket(SupportTicketDto dto)

        {

            var user = db.Users.FirstOrDefault(u => u.Username == dto.Username);

            if (user == null) return BadRequest("Invalid username");

            var ticket = new Support_Tickets

            {

                UserId = user.UserId,

                Subject = dto.Subject,

                Description = dto.Description,

                Status = "Pending", // Default status

                CreatedDate = DateTime.Now

            };

            db.Support_Tickets.Add(ticket);

            db.SaveChanges();

            dto.TicketId = ticket.TicketId;

            dto.UserId = user.UserId;

            dto.Status = "Pending";

            dto.CreatedDate = (DateTime)ticket.CreatedDate;

            return Ok(dto);

        }

        // ✅ Admin closes a ticket

        [HttpPut]

        [Route("close/{id}")]

        public IHttpActionResult CloseTicket(int id)

        {

            var ticket = db.Support_Tickets.Find(id);

            if (ticket == null) return NotFound();

            ticket.Status = "Closed";

            ticket.ClosedDate = DateTime.Now;

            db.SaveChanges();

            // 🔔 TODO: Add user notification system here

            return Ok("Ticket closed successfully");

        }

        //// ✅ User checks status

        //[HttpGet]

        //[Route("status/{username}")]

        //public IHttpActionResult GetUserTickets(string username)

        //{

        //    var user = db.Users.FirstOrDefault(u => u.Username == username);

        //    if (user == null) return NotFound();

        //    var tickets = db.Support_Tickets

        //        .Where(t => t.UserId == user.UserId)

        //        .Select(t => new

        //        {

        //            t.TicketId,

        //            t.Subject,

        //            UserStatus = (t.Status == "Closed") ? "Resolved" : "Pending",

        //            t.CreatedDate,

        //            t.ClosedDate

        //        }).ToList();

        //    return Ok(tickets);

        //}

        // ✅ User checks ticket status

        [HttpGet]

        [Route("status/{username}")]

        public IHttpActionResult GetUserTickets(string username)

        {

            var user = db.Users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null) return NotFound();

            var tickets = db.Support_Tickets

                .Where(t => t.UserId == user.UserId)

                .Select(t => new

                {

                    t.TicketId,

                    t.Subject,

                    UserStatus = (t.Status == "Closed") ? "Resolved" : t.Status, // Pending / Resolved

                    t.CreatedDate,

                    t.ClosedDate

                })

                .OrderByDescending(t => t.CreatedDate) // newest first

                .ToList();

            return Ok(tickets);

        }



    }


}


