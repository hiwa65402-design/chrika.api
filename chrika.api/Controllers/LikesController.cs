using Chrika.Api.Helpers;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/likes")]
[ApiController]
[Authorize]
public class LikesController : ControllerBase
{
    private readonly ILikeService _likeService;

    public LikesController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    [HttpPost("post/{postId}")]
    public async Task<IActionResult> TogglePostLike(int postId)
    {
        var success = await _likeService.ToggleLikeAsync(postId, "post", User.GetUserId());
        if (!success) return NotFound("Post not found.");
        return Ok();
    }

    [HttpPost("grouppost/{groupPostId}")]
    public async Task<IActionResult> ToggleGroupPostLike(int groupPostId)
    {
        var success = await _likeService.ToggleLikeAsync(groupPostId, "grouppost", User.GetUserId());
        if (!success) return NotFound("Group post not found.");
        return Ok();
    }
}
