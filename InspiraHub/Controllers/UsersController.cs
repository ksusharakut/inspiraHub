using InspiraHub.Models;
using Microsoft.AspNetCore.Mvc;

namespace InspiraHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        public string GetUsers()
        {
            return "Reading all the users";
        }

        [HttpGet("{id}")]
        public string GetUserById(int id)
        {
            return $"Reading user with ID: {id}";
        }

        [HttpPost]
        public string CreateUser([FromBody] User user)
        {
            return "Creating a user.";
        }

        [HttpPut("{id}")]
        public string UpdateUser(int id)
        {
            return $"Updating user with ID: {id}";
        }

        [HttpDelete("{id}")]
        public string DeleteUser(int id)
        {
            return $"Deleting user with ID: {id}";
        }
    }
}
