using InspiraHub.Models;
using Microsoft.AspNetCore.Mvc;

namespace InspiraHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContentsController : ControllerBase
    {
        [HttpGet]
        public string GetContent()
        {
            return "Reading all the contents";
        }

        [HttpGet("{id}")]
        public string GetContentById(int id)
        {
            return $"Reading content with ID: {id}";
        }

        [HttpPost]
        public string CreateContent([FromBody] Content content)
        {
            return "Creating a content.";
        }

        [HttpPut("{id}")]
        public string UpdateContent(int id)
        {
            return $"Updating content with ID: {id}";
        }

        [HttpDelete("{id}")]
        public string DeleteContent(int id)
        {
            return $"Deleting content with ID: {id}";
        }
    }
}
