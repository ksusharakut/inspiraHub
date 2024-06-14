using InspiraHub.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet,
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Comment>> GetCommentToContent()
        {
            List<Comment> comments = _context.Comments.ToList();

            return comments;
        }

        [HttpGet("{id:int}"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult GetCommentToContentById(int id)
        {
            if (id == 0)
            {
                return BadRequest();
                //_logger.LogError("Get user Error with Id: " + id);
            }
            Comment comment = _context.Comments.FirstOrDefault(u => u.Id == id);
            if (comment == null)
            {
                return NotFound();
            }
            return Ok(comment);
        }

        [HttpPost,
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<Comment> CreateCommentToContent([FromBody] Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (comment == null)
            {
                return BadRequest(comment);
            }
            if (comment.Id > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            comment.Id = _context.Comments.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
            _context.Comments.Add(comment);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetCommentToContentById), new { id = comment.Id }, comment);
        }

        [HttpPut("{id:int}", Name = "UpdateCommentToContent"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateCommentToContent(int id, [FromBody] Comment comment)
        {
            Comment existingComment = _context.Comments.FirstOrDefault(u => u.Id == id);
            if (existingComment == null)
            {
                return BadRequest();
            }

            existingComment.UserComment = existingComment.UserComment;
            existingComment.UserName = existingComment.UserName;
            existingComment.UserId = existingComment.UserId;
            existingComment.User = existingComment.User;
            existingComment.Content = existingComment.Content;

            _context.SaveChanges();
            Comment commentPut = _context.Comments.FirstOrDefault(cmn => cmn.Id == id);
            object result = new
            {
                User = commentPut,
                Message = "comment was successfully updated"
            };
            return Ok(result);
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialCommentToContent"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdatePartialCommentToContent(int id, JsonPatchDocument<Comment> patch)
        {
            if (patch == null || id == 0)
            {
                return BadRequest();
            }
            Comment comment = _context.Comments.FirstOrDefault(u => u.Id == id);
            if (comment == null)
            {
                return BadRequest();
            }
            patch.ApplyTo(comment, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.SaveChanges();
            Comment commentPatch = _context.Comments.FirstOrDefault(cmn => cmn.Id == id);
            object result = new
            {
                User = commentPatch,
                Message = "comment was successfully partial updated"
            };
            return Ok(result);
        }

        [HttpDelete("{id:int}", Name = "DeleteCommentToContent"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeleteCommentToContent(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            Comment comment = _context.Comments.FirstOrDefault(u => u.Id == id);
            if (comment == null)
            {
                return NotFound();
            }
            _context.Comments.Remove(comment);
            _context.SaveChanges();
            object result = new { commentDeletedId = id, Message = $"comment with ID {id} has been successfully deleted" };
            return Ok(result);
        }
    }
}
