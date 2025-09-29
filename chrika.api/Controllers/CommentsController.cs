using Chrika.Api.DTOs;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Chrika.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // POST: /api/posts/{postId}/comments
        [HttpPost("posts/{postId}/comments")]
        [Authorize]
        public async Task<IActionResult> CreateComment(int postId, CreateCommentDto createCommentDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var comment = await _commentService.CreateCommentAsync(postId, createCommentDto, int.Parse(userId));
            if (comment == null) return NotFound("Post not found.");

            return CreatedAtAction(nameof(GetCommentById), new { commentId = comment.Id }, comment);
        }

        // GET: /api/posts/{postId}/comments
        [HttpGet("posts/{postId}/comments")]
        public async Task<IActionResult> GetCommentsForPost(int postId)
        {
            var comments = await _commentService.GetCommentsForPostAsync(postId);
            return Ok(comments);
        }

        // GET: /api/comments/{commentId} (بۆ CreatedAtAction پێویستمانە)
        [HttpGet("comments/{commentId}", Name = "GetCommentById")]
        public async Task<IActionResult> GetCommentById(int commentId)
        {
            // ئەمە دەتوانین دواتر بە تەواوی جێبەجێ بکەین
            // بۆ ئێستا تەنها OK دەگەڕێنینەوە
            return Ok();
        }

        // DELETE: /api/comments/{commentId}
        [HttpDelete("comments/{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var success = await _commentService.DeleteCommentAsync(commentId, int.Parse(userId));
            if (!success) return NotFound("Comment not found or user not authorized.");

            return NoContent();
        }
    }
}
