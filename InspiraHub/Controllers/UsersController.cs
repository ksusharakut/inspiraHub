using InspiraHub.Logging;
using InspiraHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using BCrypt.Net;
using InspiraHub.Models.DTO;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using InspiraHub.Identity;

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

        [Authorize("Admin")]
        [HttpGet,
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<UserWithoutPasswordDTO>> GetUsers()
        {
            List<User> usersFromDb = _context.Users.ToList();

            List<UserWithoutPasswordDTO> usersDTO = usersFromDb.Select(u => new UserWithoutPasswordDTO
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                UpdatedAt = u.UpdatedAt,
                Name = u.Name,
                LastName = u.LastName,
                DateBirth = u.DateBirth
            }).ToList();

            _logger.Log("getting all users", "");
            return usersDTO;
        }

        [Authorize("Admin")]
        [HttpGet("{id:int}"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserWithoutPasswordDTO> GetUserById(int id)
        {
            if (id == 0)
            {
                _logger.Log("Get user Error with Id: " + id, "error");
                return BadRequest();
            }
            User user = _context.Users.FirstOrDefault(u => u.Id == id);

            if(user == null)
            {
                _logger.Log("not found user with Id: " + id, "error");
                return NotFound();
            }
            _logger.Log("getting user with id: " + id, "");

            UserWithoutPasswordDTO userDTO = new UserWithoutPasswordDTO
            {
                Id = user.Id,
                Username = user.Name,
                Email = user.Email,
                UpdatedAt = user.UpdatedAt,
                Name = user.Name,
                LastName = user.LastName,
                DateBirth = user.DateBirth
            };
            return Ok(userDTO);
        }

        [Authorize("Admin")]
        [HttpPost,
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<UserWithoutPasswordDTO> CreateUser([FromBody] User user)
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

            if (user.Id > 0)
            {
                _logger.Log("Server error: User ID should not be set manually", "error");
                return StatusCode(StatusCodes.Status500InternalServerError, "User ID should not be set manually");
            }

            user.UpdatedAt = DateTime.Now;
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            _context.Users.Add(user);
            _context.SaveChanges();
            _logger.Log("User was created", "");

            UserWithoutPasswordDTO userDTO = new UserWithoutPasswordDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                UpdatedAt = user.UpdatedAt,
                Name = user.Name,
                LastName = user.LastName,
                DateBirth = user.DateBirth
            };

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, userDTO);
        }

        [Authorize]
        [HttpPut("{id:int}", Name ="UpdateUser"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
        {
            System.Security.Claims.Claim userIdClaim = User.FindFirst("id");
            if (userIdClaim == null)
            {
                _logger.Log("User claim not found", "error");
                return Unauthorized();
            }

            int userIdFromToken;
            if (!int.TryParse(userIdClaim.Value, out userIdFromToken))
            {
                _logger.Log("Invalid user id in token", "error");
                return Unauthorized();
            }

            if (id != userIdFromToken)
            {
                _logger.Log("User tried to delete another user", "error");
                return Forbid();
            }

            User existingUser = _context.Users.FirstOrDefault(u => u.Id == id);
            if (existingUser == null)
            {
                _logger.Log("bad request", "error");
                return BadRequest();
            }

            existingUser.Username = updatedUser.Username;
            existingUser.UpdatedAt = DateTime.Now;
            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);
            }
            existingUser.Email = updatedUser.Email;
            existingUser.LastName = updatedUser.LastName;
            existingUser.DateBirth = updatedUser.DateBirth;
            existingUser.Name = updatedUser.Name;

            _context.SaveChanges();

            _logger.Log("user was successfully updated", "");
            UserWithoutPasswordDTO userDTO = new UserWithoutPasswordDTO
            {
                Id = existingUser.Id,
                Username = existingUser.Username,
                Email = existingUser.Email,
                UpdatedAt = existingUser.UpdatedAt,
                Name = existingUser.Name,
                LastName = existingUser.LastName,
                DateBirth = existingUser.DateBirth
            };
            
            object result = new
            {
                User = userDTO,
                Message = "user was successfully  updated"
            };
            return Ok(result);
            
        }

        [Authorize]
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

            System.Security.Claims.Claim userIdClaim = User.FindFirst("id");
            if (userIdClaim == null)
            {
                _logger.Log("User claim not found", "error");
                return Unauthorized();
            }

            int userIdFromToken;
            if (!int.TryParse(userIdClaim.Value, out userIdFromToken))
            {
                _logger.Log("Invalid user id in token", "error");
                return Unauthorized();
            }

            if (id != userIdFromToken)
            {
                _logger.Log("User tried to delete another user", "error");
                return Forbid();
            }

            User user = _context.Users.FirstOrDefault(u =>u.Id == id);
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
            user.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
            _logger.Log("user was successfully partial updated", "");

            UserWithoutPasswordDTO userDTO = new UserWithoutPasswordDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                UpdatedAt = user.UpdatedAt,
                Name = user.Name,
                LastName = user.LastName,
                DateBirth = user.DateBirth,

            };

            object result = new
            {
                User = userDTO,
                Message = "user was successfully partial updated"
            };

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id:int}", Name = "DeleteUser"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeleteUser(int id)
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null)
            {
                _logger.Log("User claim not found", "error");
                return Unauthorized();
            }

            int userIdFromToken;
            if (!int.TryParse(userIdClaim.Value, out userIdFromToken))
            {
                _logger.Log("Invalid user id in token", "error");
                return Unauthorized();
            }

            if (id != userIdFromToken)
            {
                _logger.Log("User tried to delete another user", "error");
                return Forbid();
            }

            User user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                _logger.Log($"User with Id {id} not found", "error");
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            _logger.Log($"User {id} was successfully deleted", "info");

            object result = new { UserDeletedId = id, Message = $"User with ID {id} has been successfully deleted" };
            return Ok(result);
        }


        [HttpGet("profile")]
        [HttpGet("{id:int}"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetProfile()
        {
            string userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid token.");
            }

            var user = _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserProfileDTO
                {
                    Username = u.Username,
                    Email = u.Email,
                    Name = u.Name,
                    LastName = u.LastName,
                    DateBirth = u.DateBirth,
                    UpdatedAt = u.UpdatedAt,
                    Role = u.Role
                })
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }
    }


}

