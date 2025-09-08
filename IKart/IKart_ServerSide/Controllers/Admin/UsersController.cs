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
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private readonly IKartEntities db = new IKartEntities();

        // GET api/users
        [HttpGet, Route("")]
        public IHttpActionResult GetUsers()
        {
            var users = db.Users.Select(u => new UserDto
            {
                UserId = u.UserId,
                UserName = u.Username,      // mapped correctly
                FullName = u.FullName,      // optional, you can show if needed
                Email = u.Email,
                PhoneNumber = u.PhoneNo,    // mapped correctly
                CreatedDate = u.CreatedDate ?? System.DateTime.MinValue, // fix nullable
                Status = u.Status
            }).ToList();

            return Ok(users);
        }

        // GET api/users/5
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetUser(int id)
        {
            var user = db.Users.Where(u => u.UserId == id).Select(u => new UserDto
            {
                UserId = u.UserId,
                UserName = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNo,
                CreatedDate = u.CreatedDate ?? System.DateTime.MinValue,
                Status = u.Status
            }).FirstOrDefault();

            if (user == null) return NotFound();

            return Ok(user);
        }

        // PUT api/users/5/status
        [HttpPut, Route("{id:int}/status")]
        public IHttpActionResult UpdateStatus(int id, [FromBody] string newStatus)
        {
            var user = db.Users.Find(id);
            if (user == null) return NotFound();

            user.Status = newStatus;
            db.SaveChanges();

            return Ok(new { Message = "Status updated successfully", Status = newStatus });
        }
    }
}
