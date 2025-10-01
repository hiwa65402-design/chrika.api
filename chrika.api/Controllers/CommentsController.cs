using Chrika.Api.DTOs;
using Chrika.Api.Helpers;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/comments")]
[ApiController]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpPost("post/{postId}")]
    public async Task<IActionResult> CreatePostComment(int postId, [FromBody] CreateCommentDto dto)
    {
        var comment = await _commentService.CreateCommentAsync(postId, "post", dto, User.GetUserId());
        if (comment == null) return NotFound("Post not found.");
        return Ok(comment);
    }

    [HttpGet("post/{postId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPostComments(int postId)
    {
        var comments = await _commentService.GetCommentsAsync(postId, "post");
        return Ok(comments);
    }

    [HttpPost("grouppost/{groupPostId}")]
    public async Task<IActionResult> CreateGroupPostComment(int groupPostId, [FromBody] CreateCommentDto dto)
    {
        var comment = await _commentService.CreateCommentAsync(groupPostId, "grouppost", dto, User.GetUserId());
        if (comment == null) return NotFound("Group post not found.");
        return Ok(comment);
    }

    [HttpGet("grouppost/{groupPostId}")]
    public async Task<IActionResult> GetGroupPostComments(int groupPostId)
    {
        var comments = await _commentService.GetCommentsAsync(groupPostId, "grouppost");
        return Ok(comments);
    }

    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var success = await _commentService.DeleteCommentAsync(commentId, User.GetUserId());
        if (!success) return Forbid("You do not have permission to delete this comment.");
        return NoContent();
    }
}
