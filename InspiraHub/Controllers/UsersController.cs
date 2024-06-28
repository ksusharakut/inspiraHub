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
using Newtonsoft.Json.Linq;

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
        public ActionResult<IEnumerable<UserWithoutPasswordDTO>> GetUsersProfiles()
        {
            List<User> usersFromDb = _context.Users.ToList();
            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole == "Admin")
            {
                List<UserWithoutPasswordDTO> usersAdminDTO = usersFromDb.Select(u => new UserWithoutPasswordDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    UpdatedAt = u.UpdatedAt,
                    Name = u.Name,
                    LastName = u.LastName,
                    DateBirth = u.DateBirth,
                    Role = u.Role,
                }).ToList();

                return Ok(usersAdminDTO);
            }
            else
            {
                List<UserGetAllUsersDTO> usersRegularUserDTO = usersFromDb.Select(u => new UserGetAllUsersDTO
                {
                    Id = u.Id,
                    Username =u.Username,
                    Email = u.Email,
                    Role = u.Role
                }).ToList();

                return Ok(usersRegularUserDTO);
            }
        }

        [Authorize]
        [HttpGet("{id:int}"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserWithoutPasswordDTO> GetUserProfileById(int id)
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

            string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole == "Admin")
            {
                UserWithoutPasswordDTO userAdminDTO = new UserWithoutPasswordDTO
                {
                    Id = user.Id,
                    Username = user.Name,
                    Email = user.Email,
                    UpdatedAt = user.UpdatedAt,
                    Name = user.Name,
                    LastName = user.LastName,
                    DateBirth = user.DateBirth
                };
                return Ok(userAdminDTO);
            }
            else
            {
                UserGetUserProfileDTO userRegularUserDTO = new UserGetUserProfileDTO
                {
                    Id=user.Id,
                    Username=user.Username,
                    Name=user.Name,
                    Email=user.Email,
                    LastName=user.LastName,
                    DateBirth = user.DateBirth,
                    Role = user.Role                    
                };
                return Ok(userRegularUserDTO);
            }
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
                DateBirth = user.DateBirth,
                Role = user.Role
            };

            return CreatedAtAction(nameof(GetUserProfileById), new { id = user.Id }, userDTO);
        }

        [Authorize]
        [HttpPatch("{id:int}", Name = "UpdateUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateUser(int id, [FromBody] JsonPatchDocument<UserUpdateDTO> patch)
        {
            System.Security.Claims.Claim userIdClaim = User.FindFirst("id");
            if (userIdClaim == null)
            {
                _logger.Log("User claim not found", "error");
                return Unauthorized();
            }

            if (!int.TryParse(userIdClaim.Value, out int userIdFromToken))
            {
                return Unauthorized();
            }

            bool isAdmin = User.IsInRole("Admin");

            if (id != userIdFromToken && !isAdmin)
            {
                return Forbid();
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var userDTO = new UserUpdateDTO
            {
                Username = user.Username,
                Email = user.Email,
                Name = user.Name,
                LastName = user.LastName,
                DateBirth = user.DateBirth,
                Role = user.Role,
                UpdatedAt = DateTime.Now,
            };

            patch.ApplyTo(userDTO, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(_context.Users.Any(u => u.Email == userDTO.Email && u.Id != id))
            {
                ModelState.AddModelError("Email", "Email already exist");
                return BadRequest(ModelState);
            }

            if (isAdmin)
            {
                // Admin can update user role
                user.Role = userDTO.Role; // Update role in database
            }

            user.Username = userDTO.Username;
            user.Email = userDTO.Email;
            user.Name = userDTO.Name;
            user.LastName = userDTO.LastName;
            user.DateBirth = userDTO.DateBirth;
            user.UpdatedAt = DateTime.Now;

            // Save changes to the database
            _context.SaveChanges();

            // Return updated user DTO
            var updatedUserDTO = new UserWithoutPasswordDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                UpdatedAt = user.UpdatedAt,
                Name = user.Name,
                LastName = user.LastName,
                DateBirth = user.DateBirth,
                Role = user.Role
            };

            var result = new
            {
                User = updatedUserDTO,
                Message = "User was successfully updated"
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

            var isAdmin = User.IsInRole("Admin");

            if (id != userIdFromToken && !isAdmin)
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
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetProfile()
        {
            var userIdClaim = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid token.");
            }

            var user = _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserProfileDTO
                {
                    Id = u.Id,
                    Username = u.Name,
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

