using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using IKart_ServerSide.Models;
using IKart_Shared.DTOs;

namespace IKart_ServerSide.Controllers.Users
{
    [RoutePrefix("api/account")]
    public class AccountController : ApiController
    {
        IKartEntities db = new IKartEntities();

        // ✅ Get user by ID
        [HttpGet]
        [Route("user/{id}")]
        public IHttpActionResult GetUser(int id)
        {
            var u = db.Users.Find(id);
            if (u == null) return NotFound();

            var dto = new UserDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNo = u.PhoneNo,
                Username = u.Username
            };

            return Ok(dto);
        }

        // ✅ Update user with validation
        [HttpPut]
        [Route("user/{id}")]
        public IHttpActionResult UpdateUser(int id, UserDto dto)
        {
            var u = db.Users.Find(id);
            if (u == null) return NotFound();

            if (string.IsNullOrWhiteSpace(dto.FullName))
                return BadRequest("Full Name is required");
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required");
            if (!Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest("Invalid Email format");
            if (string.IsNullOrWhiteSpace(dto.PhoneNo))
                return BadRequest("Phone number is required");
            if (!Regex.IsMatch(dto.PhoneNo, @"^[0-9]{10,15}$"))
                return BadRequest("Phone number must be 10–15 digits");
            if (string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest("Username is required");

            if (db.Users.Any(x => x.Email == dto.Email && x.UserId != id))
                return BadRequest("Email already exists");
            if (db.Users.Any(x => x.PhoneNo == dto.PhoneNo && x.UserId != id))
                return BadRequest("Phone number already exists");
            if (db.Users.Any(x => x.Username == dto.Username && x.UserId != id))
                return BadRequest("Username already exists");

            u.FullName = dto.FullName.Trim();
            u.Email = dto.Email.Trim();
            u.PhoneNo = dto.PhoneNo.Trim();
            u.Username = dto.Username.Trim();

            db.SaveChanges();
            return Ok(dto);
        }

        // ✅ Get all addresses for a user
        [HttpGet]
        [Route("address/user/{userId}")]
        public IHttpActionResult GetAddresses(int userId)
        {
            if (!db.Users.Any(u => u.UserId == userId))
                return BadRequest("Invalid UserId");

            var data = db.Addresses
                .Where(a => a.UserId == userId)
                .ToList()
                .Select(a => new AddressDto
                {
                    AddressId = a.AddressId,
                    UserId = (int)a.UserId,
                    Street = a.Street,
                    City = a.City,
                    State = a.State,
                    ZipCode = a.ZipCode,
                    Country = a.Country
                }).ToList();

            return Ok(data);
        }

        // ✅ Get single address
        [HttpGet]
        [Route("address/{id}")]
        public IHttpActionResult GetOneAddress(int id)
        {
            var a = db.Addresses.Find(id);
            if (a == null) return NotFound();

            var dto = new AddressDto
            {
                AddressId = a.AddressId,
                UserId = (int)a.UserId,
                Street = a.Street,
                City = a.City,
                State = a.State,
                ZipCode = a.ZipCode,
                Country = a.Country
            };

            return Ok(dto);
        }

        // ✅ Add new address with validation
        [HttpPost]
        [Route("address")]
        public IHttpActionResult AddAddress([FromBody] AddressDto dto)
        {
            if (dto.UserId <= 0) return BadRequest("UserId is required");
            if (!db.Users.Any(u => u.UserId == dto.UserId))
                return BadRequest($"Invalid UserId: {dto.UserId}. User does not exist");

            if (string.IsNullOrWhiteSpace(dto.Street))
                return BadRequest("Street is required");
            if (string.IsNullOrWhiteSpace(dto.City))
                return BadRequest("City is required");
            if (string.IsNullOrWhiteSpace(dto.State))
                return BadRequest("State is required");
            if (string.IsNullOrWhiteSpace(dto.ZipCode))
                return BadRequest("ZipCode is required");
            if (string.IsNullOrWhiteSpace(dto.Country))
                return BadRequest("Country is required");

            var a = new Address
            {
                UserId = dto.UserId,
                Street = dto.Street.Trim(),
                City = dto.City.Trim(),
                State = dto.State.Trim(),
                ZipCode = dto.ZipCode.Trim(),
                Country = dto.Country.Trim()
            };

            db.Addresses.Add(a);
            db.SaveChanges();

            dto.AddressId = a.AddressId;
            return Ok(dto);
        }

        // ✅ Update address with validation
        [HttpPut]
        [Route("address/{id}")]
        public IHttpActionResult UpdateAddress(int id, AddressDto dto)
        {
            var a = db.Addresses.Find(id);
            if (a == null) return NotFound();

            if (string.IsNullOrWhiteSpace(dto.Street))
                return BadRequest("Street is required");
            if (string.IsNullOrWhiteSpace(dto.City))
                return BadRequest("City is required");
            if (string.IsNullOrWhiteSpace(dto.State))
                return BadRequest("State is required");
            if (string.IsNullOrWhiteSpace(dto.ZipCode))
                return BadRequest("ZipCode is required");
            if (string.IsNullOrWhiteSpace(dto.Country))
                return BadRequest("Country is required");

            a.Street = dto.Street.Trim();
            a.City = dto.City.Trim();
            a.State = dto.State.Trim();
            a.ZipCode = dto.ZipCode.Trim();
            a.Country = dto.Country.Trim();

            db.SaveChanges();
            return Ok(dto);
        }

        // ✅ Delete address
        [HttpDelete]
        [Route("address/{id}")]
        public IHttpActionResult DeleteAddress(int id)
        {
            var a = db.Addresses.Find(id);
            if (a == null) return NotFound();

            db.Addresses.Remove(a);
            db.SaveChanges();

            return Ok("Deleted successfully");
        }
    }
}
