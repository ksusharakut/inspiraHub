using InspiraHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace InspiraHub.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly InspirahubContext _context;
        private IConfiguration _config;

        public AuthController(InspirahubContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] LogIn loginModel)
        {
            var user = Authenticate(loginModel);

            if (user != null)
            {
                var response = Generate(user);
                return Ok(response);
            }

            return NotFound("User not found");
        }


        private object Generate(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("id", user.Id.ToString()),
                    new Claim("name", user.Name),
                    new Claim("lastName", user.LastName),
                    new Claim("dateBirth", user.DateBirth.ToString())
            };

            var token = new JwtSecurityToken(
                Environment.GetEnvironmentVariable("JWT_ISSURE"),
                Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new
            {
                Token = tokenString,
                User = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.UpdatedAt,
                    user.Name,
                    user.LastName,
                    user.DateBirth
                }
            };
        }

        private User Authenticate(LogIn loginModel)
        {
            var currentUser = _context.Users.FirstOrDefault(o => o.Email.ToLower() ==
               loginModel.Email.ToLower());
            if (currentUser != null && BCrypt.Net.BCrypt.Verify(loginModel.Password, currentUser.Password))
            {
                return currentUser;
            }

            return null;
        }

    }
}
