// Controllers/PostsController.cs

using Chrika.Api.DTOs;
using Chrika.Api.Helpers;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chrika.Api.Controllers
{
    [Route("api/posts")] // هەموو Endpointـەکان لەژێر /api/posts دەبن
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IShareService _shareService;

        public PostsController(IPostService postService, IShareService shareService)
        {
            _postService = postService;
            _shareService = shareService;
        }

        // GET: api/posts
        // ئەمە بۆ Explore Pageـە، هەموو پۆستەکان دەهێنێت
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetAllPosts()
        {
            var userId = User.Identity.IsAuthenticated ? (int?)User.GetUserId() : null;
            var posts = await _postService.GetAllPostsAsync(userId);
            return Ok(posts);
        }

        // GET: api/posts/5
        // تاکە پۆستێک بە ID دەهێنێت
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetPost(int id)
        {
            var userId = User.Identity.IsAuthenticated ? (int?)User.GetUserId() : null;
            var post = await _postService.GetPostByIdAsync(id, userId);
            if (post == null)
            {
                return NotFound();
            }
            return Ok(post);
        }

        // POST: api/posts
        // پۆستێکی نوێ دروست دەکات
        // Controllers/PostsController.cs

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostDto createPostDto)
        {
            try
            {
                // **** چارەسەرەکە لێرەدایە ****
                // پشکنینێکی سادە: ئەگەر هیچ تێکست و هیچ وێنەیەک نەبوو، داواکارییەکە ڕەتبکەرەوە
                if (string.IsNullOrWhiteSpace(createPostDto.Content) && string.IsNullOrEmpty(createPostDto.ImageUrl))
                {
                    return BadRequest("Post cannot be empty. It must have content or an image.");
                }

                var userId = User.GetUserId();
                var createdPostDto = await _postService.CreatePostAsync(createPostDto, userId);

                return StatusCode(201, createdPostDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // PUT: api/posts/5
        // پۆستێک نوێ دەکاتەوە
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<PostDto>> UpdatePost(int id, UpdatePostDto updatePostDto)
        {
            var userId = User.GetUserId();
            var updatedPost = await _postService.UpdatePostAsync(id, updatePostDto, userId);
            if (updatedPost == null)
            {
                return NotFound("Post not found or you don't have permission to edit it.");
            }
            return Ok(updatedPost);
        }

        // DELETE: api/posts/5
        // پۆستێک دەسڕێتەوە
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userId = User.GetUserId();
            var success = await _postService.DeletePostAsync(id, userId);
            if (!success)
            {
                return NotFound("Post not found or you don't have permission to delete it.");
            }
            return NoContent();
        }

        // POST: api/posts/5/share
        // پۆستێک شێر دەکات
        [HttpPost("{postId}/share")]
        [Authorize]
        public async Task<IActionResult> SharePost(int postId)
        {
            var userId = User.GetUserId();
            await _shareService.RecordShareAsync(postId, userId);
            return Ok("Post shared successfully.");
        }
    }
}
