// Controllers/VideosController.cs

using Chrika.Api.Helpers;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Chrika.Api.Controllers
{
    [Route("api/videos")]
    [ApiController]
    public class VideosController : ControllerBase
    {
        private readonly IVideoService _videoService;

        public VideosController(IVideoService videoService)
        {
            _videoService = videoService;
        }

        /// <summary>
        /// Feedـی ڤیدیۆکان دەگەڕێنێتەوە (هاوشێوەی TikTok).
        /// </summary>
        /// <param name="pageNumber">ژمارەی لاپەڕە (بۆ نموونە: 1)</param>
        /// <param name="pageSize">قەبارەی لاپەڕە (بۆ نموونە: 10)</param>
        [HttpGet("feed")]
        [AllowAnonymous] // هەموو کەس دەتوانێت ڤیدیۆکان ببینێت
        public async Task<IActionResult> GetVideoFeed([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.Identity.IsAuthenticated ? (int?)User.GetUserId() : null;
            var videos = await _videoService.GetVideoFeedAsync(userId, pageNumber, pageSize);
            return Ok(videos);
        }
    }
}
