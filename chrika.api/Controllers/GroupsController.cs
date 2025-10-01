using Chrika.Api.DTOs;
using Chrika.Api.Helpers;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize] // هەموو کارەکانی گرووپ پێویستیان بە لۆگین هەیە
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    // POST: api/groups
    [HttpPost]
    public async Task<ActionResult<GroupDto>> CreateGroup([FromBody] CreateGroupDto createGroupDto)
    {
        // پشکنینی ئەوەی کە ئایا ناوی بەکارهێنەری گرووپەکە پێشتر گیراوە
        if (await _groupService.UsernameExistsAsync(createGroupDto.Username))
        {
            return BadRequest($"Group username '{createGroupDto.Username}' is already taken.");
        }

        var ownerId = User.GetUserId();
        var createdGroup = await _groupService.CreateGroupAsync(createGroupDto, ownerId);

        // وەڵامی 201 Created دەگەڕێنینەوە
        // return CreatedAtAction(nameof(GetGroup), new { id = createdGroup.Id }, createdGroup);
        return Ok(createdGroup); // بۆ سادەیی، ئێستا تەنها Ok دەگەڕێنینەوە
    }
    // GET: api/groups/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GroupDto>> GetGroup(int id)
    {
        var group = await _groupService.GetGroupByIdAsync(id);
        if (group == null)
        {
            return NotFound($"Group with ID {id} not found.");
        }

        // لێرەدا دەبێت پشکنینی گرووپی Private بکەین، بەڵام بۆ دواتر
        return Ok(group);
    }

    // GET: api/groups
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupDto>>> GetAllGroups()
    {
        var groups = await _groupService.GetAllPublicGroupsAsync();
        return Ok(groups);
    }



   

    // POST: api/groups/5/join
    [HttpPost("{id}/join")]
    public async Task<IActionResult> JoinGroup(int id)
    {
        var userId = User.GetUserId();
        var success = await _groupService.JoinGroupAsync(id, userId);

        if (!success)
        {
            return BadRequest("Unable to join this group. It might be private or does not exist.");
        }

        return Ok("Successfully joined the group.");
    }

    // POST: api/groups/5/leave
    [HttpPost("{id}/leave")]
    public async Task<IActionResult> LeaveGroup(int id)
    {
        var userId = User.GetUserId();
        var success = await _groupService.LeaveGroupAsync(id, userId);

        if (!success)
        {
            return BadRequest("Unable to leave this group. You might not be a member or you are the owner.");
        }

        return Ok("Successfully left the group.");
    }


    // POST: api/groups/5/follow
    [HttpPost("{id}/follow")]
    public async Task<IActionResult> FollowGroup(int id)
    {
        var userId = User.GetUserId();
        var success = await _groupService.FollowGroupAsync(id, userId);

        if (!success)
        {
            return BadRequest("Unable to follow this group. It might be private or does not exist.");
        }

        return Ok("Successfully followed the group.");
    }

    // POST: api/groups/5/unfollow
    [HttpPost("{id}/unfollow")]
    public async Task<IActionResult> UnfollowGroup(int id)
    {
        var userId = User.GetUserId();
        var success = await _groupService.UnfollowGroupAsync(id, userId);

        if (!success)
        {
            return BadRequest("You are not following this group.");
        }

        return Ok("Successfully unfollowed the group.");
    }
 

    // PUT: api/groups/5
    [HttpPut("{id}")]
    public async Task<ActionResult<GroupDto>> UpdateGroup(int id, [FromBody] UpdateGroupDto updateDto)
    {
        var userId = User.GetUserId();
        var updatedGroup = await _groupService.UpdateGroupAsync(id, updateDto, userId);

        if (updatedGroup == null)
        {
            return Forbid("You do not have permission to update this group, or the group does not exist.");
        }

        return Ok(updatedGroup);
    }


    // POST: api/groups/{groupId}/members/{userId}/promote
    [HttpPost("{groupId}/members/{userId}/promote")]
    public async Task<IActionResult> PromoteMember(int groupId, int userId)
    {
        var currentUserId = User.GetUserId();
        var success = await _groupService.PromoteMemberAsync(groupId, userId, currentUserId);

        if (!success)
        {
            return Forbid("You do not have permission to promote this member, or the user is not a valid member to be promoted.");
        }

        return Ok("Member promoted to admin successfully.");
    }

    // POST: api/groups/{groupId}/members/{userId}/demote
    [HttpPost("{groupId}/members/{userId}/demote")]
    public async Task<IActionResult> DemoteMember(int groupId, int userId)
    {
        var currentUserId = User.GetUserId();
        var success = await _groupService.DemoteMemberAsync(groupId, userId, currentUserId);

        if (!success)
        {
            return Forbid("You do not have permission to demote this admin, or the user is not an admin.");
        }

        return Ok("Admin demoted to member successfully.");
    }

    // DELETE: api/groups/{groupId}/members/{userId}
    [HttpDelete("{groupId}/members/{userId}")]
    public async Task<IActionResult> KickMember(int groupId, int userId)
    {
        var currentUserId = User.GetUserId();
        var success = await _groupService.KickMemberAsync(groupId, userId, currentUserId);

        if (!success)
        {
            return Forbid("You do not have permission to kick this member, or the member does not exist.");
        }

        return NoContent(); // 204 No Content واتە بە سەرکەوتوویی دەرکرا
    }

}
