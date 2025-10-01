using Chrika.Api.DTOs;
using Chrika.Api.Helpers; // دڵنیابە ئەمە هەیە
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
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
    // ئەم Endpointـە هەموو پۆستەکان دەگەڕێنێتەوە (بۆ Explore Page)
    // ئەگەر بەکارهێنەر لۆگینی کردبێت، زانیاری 'IsLiked' و 'IsShared'ـیشی بۆ دەنێرێت
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetAllPosts()
    {
        var userId = User.Identity.IsAuthenticated ? (int?)User.GetUserId() : null;
        var posts = await _postService.GetAllPostsAsync(userId);
        return Ok(posts);
    }

    // GET: api/posts/feed
    // ئەم Endpointـە تەنها پۆستی ئەو کەسانە دەگەڕێنێتەوە کە بەکارهێنەر فۆڵۆی کردوون
    [HttpGet("feed")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetUserFeed()
    {
        var userId = User.GetUserId();
        var feed = await _postService.GetUniversalFeedAsync(userId);
        return Ok(feed);
    }

    // GET: api/posts/5
    // وەرگرتنی تاکە پۆستێک بە ID
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
    // دروستکردنی پۆستێکی نوێ
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PostDto>> CreatePost(CreatePostDto createPostDto)
    {
        var userId = User.GetUserId();
        var createdPost = await _postService.CreatePostAsync(createPostDto, userId);
        // وەڵامی 201 Created دەگەڕێنینەوە لەگەڵ شوێنی پۆستە نوێیەکە
        return CreatedAtAction(nameof(GetPost), new { id = createdPost.Id }, createdPost);
    }

    // PUT: api/posts/5
    // نوێکردنەوەی پۆستێک
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<PostDto>> UpdatePost(int id, UpdatePostDto updatePostDto)
    {
        var userId = User.GetUserId();
        var updatedPost = await _postService.UpdatePostAsync(id, updatePostDto, userId);
        if (updatedPost == null)
        {
            // یان پۆستەکە بوونی نییە، یان هی ئەم بەکارهێنەرە نییە
            return NotFound("Post not found or you don't have permission to edit it.");
        }
        return Ok(updatedPost);
    }

    // DELETE: api/posts/5
    // سڕینەوەی پۆستێک
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
        return NoContent(); // وەڵامی 204 No Content واتە بە سەرکەوتوویی سڕایەوە
    }

    // POST: api/posts/5/share
    // Endpointـی شێرکردن
    [HttpPost("{postId}/share")]
    [Authorize]
    public async Task<IActionResult> SharePost(int postId)
    {
        var userId = User.GetUserId();
        await _shareService.RecordShareAsync(postId, userId);
        return Ok("Post shared successfully.");
    }
}
