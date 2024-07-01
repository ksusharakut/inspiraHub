using InspiraHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace InspiraHub.Controllers
{
    [Authorize("Admin")]
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
            List<Comment> comments = _context.Comments.ToList();

            return comments;
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialComment"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdatePartialComment(int id, JsonPatchDocument<Comment> patch)
        {
            if (patch == null || id == 0)
            {
                return BadRequest();
            }
            Comment comment = _context.Comments.FirstOrDefault(cmn => cmn.Id == id);
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

        [HttpDelete("{id:int}", Name = "DeleteComment"),
            Produces("application/json"),
            Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeleteComment(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            Comment comment = _context.Comments.FirstOrDefault(cmn => cmn.Id == id);
            if (comment == null)
            {
                return NotFound();
            }
            _context.Comments.Remove(comment);
            _context.SaveChanges();
            object result = new 
            { 
                commentDeletedId = id, 
                Message = $"comment with ID {id} has been successfully deleted" 
            };
            return Ok(result);
        }
    }
}
