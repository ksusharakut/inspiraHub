using InspiraHub.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace InspiraHub.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly InspirahubContext _context;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(InspirahubContext context, ILogger<CommentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Comment>> GetComment()
        {
            var comments = _context.Comments.ToList();

            return comments;
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult GetCommentById(int id)
        {
            if (id == 0)
            {
                return BadRequest();
                //_logger.LogError("Get user Error with Id: " + id);
            }
            var comment = _context.Comments.FirstOrDefault(u => u.Id == id);
            if (comment == null)
            {
                return NotFound();
            }
            return Ok(comment);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<Comment> CreateComment([FromBody] Comment comment)
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
            return CreatedAtAction(nameof(GetCommentById), new { id = comment.Id }, comment);
        }

        [HttpPut("{id:int}", Name = "UpdateComment")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateComment(int id, [FromBody] Comment comment)
        {
            var existingComment = _context.Comments.FirstOrDefault(u => u.Id == id);
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
            return NoContent();
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialComment")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdatePartialComment(int id, JsonPatchDocument<Comment> patch)
        {
            if (patch == null || id == 0)
            {
                return BadRequest();
            }
            var comment = _context.Comments.FirstOrDefault(u => u.Id == id);
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
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "DeleteComment")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeleteComment(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var comment = _context.Comments.FirstOrDefault(u => u.Id == id);
            if (comment == null)
            {
                return NotFound();
            }
            _context.Comments.Remove(comment);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
