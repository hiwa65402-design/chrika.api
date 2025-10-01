// Controllers/LikesController.cs

using Chrika.Api.Helpers;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Chrika.Api.Controllers
{
    [Route("api/likes")] // ڕێڕەوی بنەڕەتی گۆڕا بۆ /api/likes
    [ApiController]
    [Authorize] // هەموو کارەکانی لایک پێویستی بە لۆگین هەیە
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikesController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        /// <summary>
        /// لایک/ئەنلایکی پۆستێکی ئاسایی دەکات.
        /// </summary>
        [HttpPost("post/{postId}")]
        public async Task<IActionResult> TogglePostLike(int postId)
        {
            // فانکشنە نوێیەکە بانگ دەکەین
            // groupPostId بە null دەنێرین
            var success = await _likeService.ToggleLikeAsync(postId, null, User.GetUserId());

            if (!success)
            {
                return NotFound("Post not found or user not found.");
            }

            return Ok();
        }

        /// <summary>
        /// لایک/ئەنلایکی پۆستێکی گرووپ دەکات.
        /// </summary>
        [HttpPost("grouppost/{groupPostId}")]
        public async Task<IActionResult> ToggleGroupPostLike(int groupPostId)
        {
            // فانکشنە نوێیەکە بانگ دەکەین
            // postId بە null دەنێرین
            var success = await _likeService.ToggleLikeAsync(null, groupPostId, User.GetUserId());

            if (!success)
            {
                return NotFound("Group post not found or user not found.");
            }

            return Ok();
        }
    }
}
