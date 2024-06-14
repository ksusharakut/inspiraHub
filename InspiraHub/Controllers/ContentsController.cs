using InspiraHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace InspiraHub.Controllers
{
    [ApiController]
    [Route("api/contents")]
    public class ContentsController : ControllerBase
    {
        private readonly InspirahubContext _context;
        private readonly ILogger<ContentsController> _logger;

        public ContentsController(InspirahubContext context, ILogger<ContentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Content>> GetContent()
        {
            List<Content> contents = _context.Contents.ToList();
            _logger.LogInformation("getting all contents");
            return contents;
        }

        [HttpGet("{id:int}"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult GetContentById(int id)
        {
            if (id == 0)
            {
                _logger.LogError("Get content Error with Id: " + id);
                return BadRequest();
            }
            Content content = _context.Contents.FirstOrDefault(cnt => cnt.Id == id);
            if (content == null)
            {
                return NotFound();
            }
            return Ok(content);
        }

        [Authorize]
        [HttpPost,
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<Content> CreateContent([FromBody] Content content)
        {
            if (content == null)
            {
                return BadRequest("Content is null.");
            }
            if (content.Id > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Content ID should not be set.");
            }

            // Получение идентификатора пользователя из JWT-токена
            System.Security.Claims.Claim userIdClaim = User.FindFirst("id");
            if (userIdClaim == null)
            {
                return Unauthorized("User claim not found in token.");
            }

            if (!long.TryParse(userIdClaim.Value, out long userIdFromToken))
            {
                return Unauthorized("Invalid user id in token.");
            }

            // Получение пользователя из базы данных
            User user = _context.Users.Find(userIdFromToken);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // Установка полей
            content.UserId = userIdFromToken;
            content.User = user;
            content.CreateAt = DateTime.Now;

            _context.Contents.Add(content);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetContentById), new { id = content.Id }, content);
        }

        [HttpPut("{id:int}", Name = "UpdateContent"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateContent(int id, [FromBody] Content content)
        {
            Content existingContent = _context.Contents.FirstOrDefault(cnt => cnt.Id == id);
            if (existingContent == null)
            {
                return BadRequest();
            }

            existingContent.Preview = existingContent.Preview;
            existingContent.User = existingContent.User;
            existingContent.UserId = existingContent.UserId;
            existingContent.Title = existingContent.Title;  
            existingContent.Description = existingContent.Description;
            existingContent.CreateAt = DateTime.UtcNow;
            existingContent.ContentType = existingContent.ContentType;

            _context.SaveChanges();
            Content contentPut = _context.Contents.FirstOrDefault(cnt => cnt.Id == id);
            object result = new
            {
                Content = contentPut,
                Message = "content was successfully updated"
            };
            return Ok(result);
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialContent"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdatePartialContent(int id, JsonPatchDocument<Content> patch)
        {
            if (patch == null || id == 0)
            {
                return BadRequest();
            }
            Content content = _context.Contents.FirstOrDefault(cnt => cnt.Id == id);
            if (content == null)
            {
                return BadRequest();
            }
            patch.ApplyTo(content, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.SaveChanges();
            Content contentPatch = _context.Contents.FirstOrDefault(cnt => cnt.Id == id);
            object result = new
            {
                Content = contentPatch,
                Message = "content was successfully partial updated"
            };
            return Ok(result);
        }


        [Authorize]
        [HttpDelete("{id:int}", Name = "DeleteContent"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeleteContent(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            Content content = _context.Contents.FirstOrDefault(cnt => cnt.Id == id);
            if (content == null)
            {
                return NotFound();
            }
            _context.Contents.Remove(content);
            _context.SaveChanges();
            object result = new { ContentDeletedId = id, Message = $"Content with ID {id} has been successfully deleted" };
            return Ok(result);
        }
    }
}
