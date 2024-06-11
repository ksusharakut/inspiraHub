using InspiraHub.Models;
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
            var contents = _context.Contents.ToList();
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
            var content = _context.Contents.FirstOrDefault(cnt => cnt.Id == id);
            if (content == null)
            {
                return NotFound();
            }
            return Ok(content);
        }

        [HttpPost,
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<Content> CreateContent([FromBody] Content content)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (content == null)
            {
                return BadRequest(content);
            }
            if (content.Id > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            content.Id = _context.Contents.OrderByDescending(cnt => cnt.Id).FirstOrDefault().Id + 1;
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
            var existingContent = _context.Contents.FirstOrDefault(cnt => cnt.Id == id);
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
            var contentPut = _context.Contents.FirstOrDefault(cnt => cnt.Id == id);
            var result = new
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
            var content = _context.Contents.FirstOrDefault(cnt => cnt.Id == id);
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
            var contentPatch = _context.Contents.FirstOrDefault(cnt => cnt.Id == id);
            var result = new
            {
                Content = contentPatch,
                Message = "content was successfully partial updated"
            };
            return Ok(result);
        }

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
            var content = _context.Contents.FirstOrDefault(cnt => cnt.Id == id);
            if (content == null)
            {
                return NotFound();
            }
            _context.Contents.Remove(content);
            _context.SaveChanges();
            var result = new { ContentDeletedId = id, Message = $"Content with ID {id} has been successfully deleted" };
            return Ok(result);
        }
    }
}
