using Chrika.Api.DTOs;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    // GET: api/posts - وەرگرتنی هەموو پۆستەکان (بۆ هەمووان کراوەیە)
    [HttpGet]
    public async Task<IActionResult> GetPosts()
    {
        var posts = await _postService.GetAllPostsAsync();
        return Ok(posts);
    }

    // POST: api/posts - دروستکردنی پۆستی نوێ (تەنها بۆ بەکارهێنەری لۆگینبوو)
    [HttpPost]
    [Authorize] // <-- پاراستنی Endpoint
    public async Task<IActionResult> CreatePost(CreatePostDto createPostDto)
    {
        // وەرگرتنی IDی بەکارهێنەر لە تۆکنەکە
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var createdPost = await _postService.CreatePostAsync(createPostDto, int.Parse(userId));

        // return CreatedAtAction(nameof(GetPost), new { id = createdPost.Id }, createdPost);
        return Ok(createdPost); // بۆ سادەیی
    }
}
