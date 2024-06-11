﻿using InspiraHub.Logging;
using InspiraHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using BCrypt.Net;

namespace InspiraHub.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly InspirahubContext _context;
        private readonly ILogging _logger;

        public UsersController(InspirahubContext context, ILogging logger)
        {
            _context = context;
            _logger = logger;
        }

        [Authorize]
        [HttpGet,
             Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            var users = _context.Users.ToList();
            _logger.Log("getting all users", "");
            return users;
        }

        [HttpGet("{id:int}"),
                         Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult GetUserById(int id)
        {
            if (id == 0)
            {
                _logger.Log("Get user Error with Id: " + id, "error");
                return BadRequest();
            }
            var users = _context.Users.FirstOrDefault(u => u.Id == id);
            if(users == null)
            {
                _logger.Log("not found user with Id: " + id, "error");
                return NotFound();
            }
            _logger.Log("getting user with id: " + id, "");
            return Ok(users);
        }

        [HttpPost,
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<User> CreateUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                _logger.Log("Bad request due to invalid model state", "error");
                return BadRequest(ModelState);
            }

            if (_context.Users.Any(u => u.Email.ToLower() == user.Email.ToLower()))
            {
                ModelState.AddModelError("CustomError", "User already exists!");
                _logger.Log("User already exists", "error");
                return BadRequest(ModelState);
            }

            if (user == null)
            {
                _logger.Log("Bad request due to null user", "error");
                return BadRequest("User cannot be null");
            }

            // Assuming the ID is automatically generated by the database.
            // No need to manually set the ID.
            if (user.Id > 0)
            {
                _logger.Log("Server error: User ID should not be set manually", "error");
                return StatusCode(StatusCodes.Status500InternalServerError, "User ID should not be set manually");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            _context.Users.Add(user);
            _context.SaveChanges();
            _logger.Log("User was created", "");

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        [HttpPut("{id:int}", Name ="UpdateUser"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateUser(int id, [FromBody]User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Id == id);
            if (existingUser == null)
            {
                _logger.Log("bad request", "error");
                return BadRequest();
            }

            existingUser.Username = existingUser.Username;
            existingUser.UpdatedAt = DateTime.Now;
            existingUser.Password = existingUser.Password;
            existingUser.Email = existingUser.Email;
            existingUser.LastName = existingUser.LastName;
            existingUser.DateBirth = existingUser.DateBirth;
            existingUser.Name = existingUser.Name;

            _context.SaveChanges();

            _logger.Log("user was successfully updated", "");
            var userPut = _context.Users.FirstOrDefault(u => u.Id == id);
            var result = new
            {
                User = userPut,
                Message = "user was successfully  updated"
            };
            return Ok(result);
            
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialUser"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdatePartialUser(int id, JsonPatchDocument<User> patch)
        {
            if (patch == null || id == 0)
            {
                _logger.Log("bad request", "error");
                return BadRequest();
            }
            var user = _context.Users.FirstOrDefault(u =>u.Id == id);
            if(user == null)
            {
                _logger.Log("bad request", "error");
                return BadRequest();
            }
            patch.ApplyTo(user, ModelState);
            if (!ModelState.IsValid)
            {
                _logger.Log("bad request", "error");
                return BadRequest(ModelState);
            }
            _context.SaveChanges();
            _logger.Log("user was successfully partial updated", "");

            var userPatch = _context.Users.FirstOrDefault(u => u.Id == id);
            var result = new
            {
                User = userPatch,
                Message = "user was successfully partial updated"
            };
            return Ok(result);
        }

        //[Authorize]
        [HttpDelete("{id:int}", Name = "DeleteUser"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeleteUser(int id)
        {
            if (id == 0)
            {
                _logger.Log("bad request", "error");
                return BadRequest();
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                _logger.Log("not found user with Id: " + id, "error");
                return NotFound();
            }
            _context.Users.Remove(user);
            _context.SaveChanges();
            _logger.Log("user was successfully deleted: " + id, "");
            var result = new { UserDeletedId = id, Message = $"User with ID {id} has been successfully deleted" };
            return Ok(result);
        }
    }


}

