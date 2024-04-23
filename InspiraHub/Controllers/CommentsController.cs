using InspiraHub.Models;
using Microsoft.AspNetCore.Mvc;

namespace InspiraHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController
    {
        [HttpGet]
        public string GetComment()
        {
            return "Reading all the comments";
        }

        [HttpGet("{id}")]
        public string GetCommentById(int id)
        {
            return $"Reading comment with ID: {id}";
        }

        [HttpPost]
        public string CreateComment([FromBody] Comment comment)
        {
            return "Creating a comment.";
        }

        [HttpPut("{id}")]
        public string UpdateComment(int id)
        {
            return $"Updating comment with ID: {id}";
        }

        [HttpDelete("{id}")]
        public string DeleteComment(int id)
        {
            return $"Deleting comment with ID: {id}";
        }
    }
}
