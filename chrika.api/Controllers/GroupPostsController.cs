using Chrika.Api.DTOs;
using Chrika.Api.Helpers;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/groups/{groupId}/posts")]
[ApiController]
[Authorize]
public class GroupPostsController : ControllerBase
{
    private readonly IGroupPostService _groupPostService;

    public GroupPostsController(IGroupPostService groupPostService)
    {
        _groupPostService = groupPostService;
    }

    // POST: api/groups/5/posts
    [HttpPost]
    public async Task<ActionResult<GroupPostDto>> CreatePost(int groupId, [FromBody] CreateGroupPostDto createDto)
    {
        var authorId = User.GetUserId();
        var post = await _groupPostService.CreateGroupPostAsync(groupId, createDto, authorId);

        if (post == null)
        {
            return Forbid("You must be a member of the group to post.");
        }

        return CreatedAtAction(nameof(GetPost), new { groupId, id = post.Id }, post);
    }

    // GET: api/groups/5/posts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupPostDto>>> GetPosts(int groupId)
    {
        var userId = User.GetUserId();
        var posts = await _groupPostService.GetPostsForGroupAsync(groupId, userId);

        if (posts == null)
        {
            return Forbid("You do not have permission to view posts in this private group.");
        }

        return Ok(posts);
    }

    // GET: api/groups/5/posts/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPost(int groupId, int id)
    {
        // ئەمە بۆ دواتر جێبەجێ دەکەین
        return Ok($"Details for post {id} in group {groupId}");
    }
}
