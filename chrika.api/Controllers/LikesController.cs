using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Chrika.Api.Controllers
{
    [Route("api/posts/{postId}/like")]
    [ApiController]
    [Authorize] // هەموو کارەکانی لایک پێویستی بە لۆگین هەیە
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikesController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        // POST: api/posts/5/like
        [HttpPost]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var success = await _likeService.ToggleLikeAsync(postId, int.Parse(userId));

            if (!success)
            {
                return NotFound("Post not found.");
            }

            return Ok();
        }
    }
}
