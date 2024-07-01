using InspiraHub.Models;
using InspiraHub.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ContentDTO>> GetContent()
        {
            List<ContentDTO> contents = _context.Contents
                .Select(c => new ContentDTO
                {
                    Id = c.Id,
                    Title = c.Title,
                    ContentType = c.ContentType,
                    CreateAt = DateTime.Now,
                    Preview = c.Preview,
                    Description = c.Description,
                    UserId = c.UserId
                })
                .ToList();
            _logger.LogInformation("getting all contents");
            return contents;
        }

        [Authorize]
        [HttpGet("{id:int}"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ContentDTO> GetContentById(int id)
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

            ContentDTO contentDTO = new ContentDTO
            {
                Id = content.Id,
                Title = content.Title,
                ContentType = content.ContentType,
                CreateAt = DateTime.Now,
                Preview = content.Preview,
                Description = content.Description,
                UserId = content.UserId
            };
            return Ok(contentDTO);
        }

        [Authorize]
        [HttpPost]
            [Produces("application/json")]
            [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ContentDTO> CreateContent([FromBody] ContentDTO contentDTO)
        {
            if (contentDTO == null)
            {
                return BadRequest("Content is null");
            }

            if (contentDTO.Id > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Content ID should not be set.");
            }

            System.Security.Claims.Claim userIdClaim = User.FindFirst("id");

            if (userIdClaim == null)
            {
                return Unauthorized("User claim not found in token.");
            }

            if (!long.TryParse(userIdClaim.Value, out long userIdFromToken))
            {
                return Unauthorized("Invalid user id in token.");
            }

            User user = _context.Users.Find(userIdFromToken);

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            Content content = new Content
            {
                UserId = userIdFromToken,
                User = user,
                Preview = contentDTO.Preview,
                Title = contentDTO.Title,
                Description = contentDTO.Description,
                CreateAt = DateTime.Now,
                ContentType = contentDTO.ContentType
            };

            _context.Contents.Add(content);
            _context.SaveChanges();

            ContentDTO responceContentDTO = new ContentDTO
            {
                Id = content.Id,
                UserId = content.UserId,
                Preview = content.Preview,
                Title = content.Title,
                Description = content.Description,
                CreateAt = content.CreateAt,
                ContentType = content.ContentType
            };
            return CreatedAtAction(nameof(GetContentById), new 
            { 
                id = responceContentDTO.Id 
            }, 
            responceContentDTO);
        }

        [Authorize]
        [HttpPatch("{id:int}", Name = "UpdatePartialContent"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdatePartialContent(int id, [FromBody] JsonPatchDocument<Content> patch)
        {
            if (patch == null || id == 0)
            {
                return BadRequest();
            }

            System.Security.Claims.Claim userIdClaim = User.FindFirst("id");
            if(userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userIdFromToken))
            {
                return Unauthorized("Invalid user claim.");
            }

            Content content = _context.Contents.FirstOrDefault(cnt => cnt.Id == id);
            if (content == null)
            {
                return NotFound();
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = content.UserId == userIdFromToken;

            if(!isAdmin && !isOwner)
            {
                return Forbid("you are not allowed to update this content");
            }

            patch.ApplyTo(content, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.SaveChanges();

            ContentDTO contentDTO = new ContentDTO()
            {
                Id = content.Id,
                UserId = content.UserId,
                Preview = content.Preview,
                Title = content.Title,
                Description = content.Description,
                CreateAt = content.CreateAt,
                ContentType = content.ContentType
            };

            object result = new
            {
                Content = contentDTO,
                Message = "content was successfully partial updated"
            };
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id:int}", Name = "DeleteContent")]
        [Produces("application/json")]
        public IActionResult DeleteContent(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            System.Security.Claims.Claim userIdClaim = User.FindFirst("id");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userIdFromToken))
            {
                return Unauthorized("Invalid user claim.");
            }

            Content content = _context.Contents.FirstOrDefault(cnt => cnt.Id == id);
            if (content == null)
            {
                return NotFound();
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = content.UserId == userIdFromToken;

            if (!isAdmin && !isOwner)
            {
                return Forbid();
            }

            _context.Contents.Remove(content);
            _context.SaveChanges();

            object result = new
            {
                ContentDeletedId = id,
                Message = $"Content with ID {id} has been successfully deleted"
            };
            return Ok(result);
        }
    }
}
