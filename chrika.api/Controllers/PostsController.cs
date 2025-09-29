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
    //[HttpGet("feed")]
    //[Authorize] // تەنها بەکارهێنەری لۆگینبوو دەتوانێت Feed ـی خۆی ببینێت
    //public async Task<IActionResult> GetUserFeed()
    //{
    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //    if (userId == null) return Unauthorized();

    //    var feed = await _postService.GetFeedForUserAsync(int.Parse(userId));
    //    return Ok(feed);
    //}

    // GET: api/posts - وەرگرتنی هەموو پۆستەکان (Explore Page)
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
    [HttpDelete("{id}")] 
    [Authorize]
    public async Task<IActionResult> DeletePost(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var success = await _postService.DeletePostAsync(id, int.Parse(userId));

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
    // PUT: api/posts/5
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePost(int id, UpdatePostDto updatePostDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var updatedPost = await _postService.UpdatePostAsync(id, updatePostDto, int.Parse(userId));

        if (updatedPost == null)
        {
            // وەک Delete، ئەمە یان پۆستەکە نییە یان هی ئەم بەکارهێنەرە نییە
            return NotFound();
        }

        return Ok(updatedPost);
    }

}
