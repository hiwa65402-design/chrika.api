// Controllers/FeedController.cs

using Chrika.Api.Helpers;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Chrika.Api.Controllers
{
    [Route("api/feed")]
    [ApiController]
    public class FeedController : ControllerBase
    {
        private readonly IPostService _postService;

        // ئێستا تەنها IPostService وەردەگرێت
        public FeedController(IPostService postService)
        {
            _postService = postService;
        }

        /// <summary>
        /// Feedـی تایبەت بە بەکارهێنەری لۆگینبوو دەگەڕێنێتەوە (لەگەڵ ڕیکلام).
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyFeed()
        {
            var userId = User.GetUserId();
            var feed = await _postService.GetUniversalFeedAsync(userId);
            return Ok(feed);
        }

        /// <summary>
        /// Feedـێکی گشتی دەگەڕێنێتەوە بۆ کەسانی بێ لۆگین (لەگەڵ ڕیکلامی گشتی).
        /// </summary>
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicFeed()
        {
            var feed = await _postService.GetUniversalFeedAsync(null);
            return Ok(feed);
        }
    }
}
