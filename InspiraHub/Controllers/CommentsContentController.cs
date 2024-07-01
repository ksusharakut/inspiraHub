using InspiraHub.Models;
using InspiraHub.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace InspiraHub.Controllers
{
    [ApiController]
    [Route("api/contents/{contentId}/comments")]
    public class CommentsContentController : ControllerBase
    {
        private readonly InspirahubContext _context;
        private readonly ILogger<CommentsContentController> _logger;

        public CommentsContentController(InspirahubContext context, ILogger<CommentsContentController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [Authorize]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<CommentDTO>> GetCommentToContent()
        {
            var comments = _context.Comments.ToList();

            var commentDTOs = comments.Select(c => new CommentDTO
            {
                Id = c.Id,
                UserId = c.UserId,
                ContentId = c.ContentId,
                UserComment = c.UserComment,
                CreateAt = c.CreateAt,
                UserName = c.UserName
            }).ToList();

            return Ok(commentDTOs);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<CommentDTO> GetCommentToContentById(long contentId, int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            Comment comment = _context.Comments.FirstOrDefault(u => u.Id == id && u.ContentId == contentId);

            if (comment == null)
            {
                return NotFound();
            }

            CommentDTO commentDTO = new CommentDTO
            {
                Id = comment.Id,
                UserId = comment.UserId,
                ContentId = comment.ContentId,
                UserComment = comment.UserComment,
                CreateAt = comment.CreateAt,
                UserName = comment.UserName
            };

            return Ok(commentDTO);
        }

        [Authorize]
        [HttpPost,
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult CreateCommentToContent(long contentId, [FromBody] CommentInputModelDTO commentInputDTO)
        {
            Content content = _context.Contents.FirstOrDefault(c => c.Id == contentId);
            if(content == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            System.Security.Claims.Claim userIdClaim = User.FindFirst("id");
            System.Security.Claims.Claim userNameClaim = User.FindFirst("username");

            if (userIdClaim == null || userNameClaim == null)
            {
                return Unauthorized("User claim not found in token.");
            }

            if (!long.TryParse(userIdClaim.Value, out long userIdFromToken))
            {
                return Unauthorized("Invalid user id in token.");
            }

            Comment newComment = new Comment
{
                UserId = userIdFromToken,
                ContentId = contentId,
                UserComment = commentInputDTO.UserComment,
                UserName = userNameClaim.Value,
                CreateAt = DateTime.Now
            };

            _context.Comments.Add(newComment);
            _context.SaveChanges();

            CommentDTO commentDTO = new CommentDTO
            {
                Id = newComment.Id,
                UserId = newComment.UserId,
                ContentId = contentId,
                UserComment = newComment.UserComment,
                CreateAt = newComment.CreateAt,
                UserName = newComment.UserName
            };

            return CreatedAtAction(nameof(GetCommentToContentById), new 
            {
                contentId = contentId,
                id = newComment.Id },
                commentDTO);
        }

        [Authorize]
        [HttpPatch("{id:int}", Name = "UpdateCommentToContent")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult UpdatePartialCommentToContent(int id, [FromBody] JsonPatchDocument<Comment> patch)
        {
            if (patch == null || id == 0)
            {
                return BadRequest();
            }

            System.Security.Claims.Claim userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userIdFromToken))
            {
                return Unauthorized("Invalid user claim.");
            }

            Comment comment = _context.Comments.FirstOrDefault(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = comment.UserId == userIdFromToken;

            if (!isAdmin && !isOwner)
            {
                return Forbid("You are not allowed to update this comment");
            }

            patch.ApplyTo(comment, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.SaveChanges();

            CommentDTO commentDTO = new CommentDTO()
            {
                Id = comment.Id,
                UserId = comment.UserId,
                ContentId = comment.ContentId, 
                UserComment = comment.UserComment,
                CreateAt = comment.CreateAt,
                UserName = comment.UserName
            };

            object result = new
            {
                Comment = commentDTO,
                Message = "Comment was successfully partially updated"
            };
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id:int}", Name = "DeleteCommentToContent"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeleteCommentToContent(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (id == 0)
            {
                return BadRequest();
            }

            Comment comment = _context.Comments.FirstOrDefault(u => u.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            if (comment.UserId.ToString() != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            _context.SaveChanges();

            object result = new { commentDeletedId = id, Message = $"Comment with ID {id} has been successfully deleted" };
            return Ok(result);
        }
    }
}
