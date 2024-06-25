using InspiraHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net;
using InspiraHub.Models.DTO;



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
        
        [HttpPost("registrate"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<User> RegistrateUser([FromBody] UserRegistrationDTO userDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Users.Any(u => u.Email.ToLower() == userDTO.Email.ToLower()))
            {
                ModelState.AddModelError("CustomError", "User already exists!");
                return BadRequest(ModelState);
            }

            if (userDTO == null)
            {
                return BadRequest("User cannot be null");
            }
            User user = new User
            {
                Username = userDTO.Username,
                Email = userDTO.Email,
                Name = userDTO.Name,
                LastName = userDTO.LastName,
                DateBirth = userDTO.DateBirth,
                Password = BCrypt.Net.BCrypt.HashPassword(userDTO.Password),
                Role = "RegularUser",
                UpdatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            UserWithoutPasswordDTO requestUser = new UserWithoutPasswordDTO
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                LastName = user.LastName,
                DateBirth = user.DateBirth,
                Username = user.Username,
                UpdatedAt= user.UpdatedAt,
                Role = user.Role
            };

            return CreatedAtAction(nameof(Login), new { id = user.Id }, requestUser);
        }

        [HttpPost("forgot_password"),
             Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SendEmail([FromBody] UserEmailDTO sendEmail)
        {
            if (string.IsNullOrEmpty(sendEmail.Email) || !IsValidEmail(sendEmail.Email))
            {
                return BadRequest(new { error = "Неверный формат электронной почты." });
            }
            try
            {
                string token = GenerateCode();
                PasswordResetToken passResetToken = new PasswordResetToken
                {
                    Email = sendEmail.Email,
                    Token = token,
                    CreatedAt = DateTime.Now
                };

                _context.PasswordResetTokens.Add(passResetToken);
                _context.SaveChanges();

                string smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER");
                string port = Environment.GetEnvironmentVariable("SMTP_PORT");

                string username = Environment.GetEnvironmentVariable("USERNAME");
                string password = Environment.GetEnvironmentVariable("PASSWORD");

                using (SmtpClient client = new SmtpClient(smtpServer, Convert.ToInt16(port)))
                {
                    client.Credentials = new NetworkCredential(username, password);
                    client.EnableSsl = false;

                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(Environment.GetEnvironmentVariable("FROM_EMAIL"));
                    message.To.Add(passResetToken.Email);
                    message.Subject = "Код восстановления пароля";
                    message.Body = $"Ваш код восстановления: {token}";

                    client.Send(message);
                    return Ok(new { message = "Письмо с кодом отправлено." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке письма: {ex.Message}");
                return BadRequest(new { error = "Не удалось отправить письмо." });
            }
        }

        [HttpPost("reset_password")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ConfirmPassword([FromBody] PasswordRecoveryDTO passRecovery)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (passRecovery.NewPassword != passRecovery.RepeatPassword)
            {
                ModelState.AddModelError("RepeatPassword", "Пароли не совпадают.");
                return BadRequest(ModelState);
            }

            User user = GetUserByEmail(passRecovery.Email, passRecovery.Token);
            if (user == null)
            {
                return NotFound(new { error = "Пользователь не найден или неверный код, попробуйте заново или вернитесь к генерации кода подтверждения" });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(passRecovery.NewPassword);
            user.UpdatedAt = DateTime.Now;

            _context.Users.Update(user);
            _context.SaveChanges();

            PasswordResetToken token = _context.PasswordResetTokens.FirstOrDefault(t => t.Email == passRecovery.Email && t.Token == passRecovery.Token);
            if (token != null)
            {
                _context.PasswordResetTokens.Remove(token);
                _context.SaveChanges();
            }

            return Ok(new { message = "Пароль успешно обновлен." });
        }

        [AllowAnonymous]
        [HttpPost("login"),
            Produces("application/json"),
            Consumes("application/json")]
        public IActionResult Login([FromBody] UserLogInDTO loginModel)
        {
            User user = Authenticate(loginModel);

            if (user != null)
            {
                object response = Generate(user);
                return Ok(response);
            }

            return NotFound("User not found");
        }

        //[Authorize]
        //[HttpPost("logout")]
        //public IActionResult Logout()
        //{
        //    string authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        //    if (authHeader.StartsWith("Bearer "))
        //    {
        //        string token = authHeader.Substring("Bearer ".Length);

        //        JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
        //        DateTime? expiration = jwtToken?.ValidTo;

        //        if (expiration.HasValue)
        //        {
        //            _memoryCache.Set(token, token, expiration.Value);
        //        }
        //    }
        //    return Ok(new { message = "Logged out successfully." });
        //}

        private object Generate(User user)
        {
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            Claim[] claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("id", user.Id.ToString()),
                    new Claim("name", user.Name),
                    new Claim("lastName", user.LastName),
                    new Claim("dateBirth", user.DateBirth.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
            };

            JwtSecurityToken token = new JwtSecurityToken(
                Environment.GetEnvironmentVariable("JWT_ISSUER"),
                Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials);
            
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

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
                    user.DateBirth,
                    user.Role
                }
            };
        }

        private User Authenticate(UserLogInDTO loginModel)
        {
            User currentUser = _context.Users.FirstOrDefault(u => u.Email.ToLower() ==
               loginModel.Email.ToLower());
            if (currentUser != null && BCrypt.Net.BCrypt.Verify(loginModel.Password, currentUser.Password))
            {
                return currentUser;
            }
            return null;
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                System.Net.Mail.MailAddress addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        private string GenerateCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); 
        }
        public User GetUserByEmail(string email, string token)
        {
            PasswordResetToken passwordResetToken = _context.PasswordResetTokens.FirstOrDefault(t => t.Email == email && t.Token == token);

            if (passwordResetToken != null)
            {
                DateTime createdAtUtc = passwordResetToken.CreatedAt.ToUniversalTime().Date;
                return _context.Users.FirstOrDefault(u => u.Email == email);
            }
            return null;
        }
    }
}
