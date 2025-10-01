using Chrika.Api.DTOs;
using Chrika.Api.Helpers;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpPost]
    public async Task<ActionResult<GroupDto>> CreateGroup([FromBody] CreateGroupDto createGroupDto)
    {
        if (await _groupService.UsernameExistsAsync(createGroupDto.Username))
        {
            return BadRequest($"Group username '{createGroupDto.Username}' is already taken.");
        }
        var ownerId = User.GetUserId();
        var createdGroup = await _groupService.CreateGroupAsync(createGroupDto, ownerId);
        return Ok(createdGroup);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GroupDto>> GetGroup(int id)
    {
        var group = await _groupService.GetGroupByIdAsync(id);
        if (group == null)
        {
            return NotFound($"Group with ID {id} not found.");
        }
        return Ok(group);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupDto>>> GetAllGroups()
    {
        var groups = await _groupService.GetAllPublicGroupsAsync();
        return Ok(groups);
    }

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

    [HttpPut("{id}")]
    public async Task<ActionResult<GroupDto>> UpdateGroup(int id, [FromBody] UpdateGroupDto updateDto)
    {
        var userId = User.GetUserId();
        var updatedGroup = await _groupService.UpdateGroupAsync(id, updateDto, userId);
        if (updatedGroup == null)
        {
            return StatusCode(403, "You do not have permission to update this group, or the group does not exist.");
        }
        return Ok(updatedGroup);
    }

    [HttpPost("{groupId}/members/{userId}/promote")]
    public async Task<IActionResult> PromoteMember(int groupId, int userId)
    {
        var currentUserId = User.GetUserId();
        var success = await _groupService.PromoteMemberAsync(groupId, userId, currentUserId);
        if (!success)
        {
            return StatusCode(403, "You do not have permission to promote this member, or the user is not a valid member to be promoted.");
        }
        return Ok("Member promoted to admin successfully.");
    }

    [HttpPost("{groupId}/members/{userId}/demote")]
    public async Task<IActionResult> DemoteMember(int groupId, int userId)
    {
        var currentUserId = User.GetUserId();
        var success = await _groupService.DemoteMemberAsync(groupId, userId, currentUserId);
        if (!success)
        {
            return StatusCode(403, "You do not have permission to demote this admin, or the user is not an admin.");
        }
        return Ok("Admin demoted to member successfully.");
    }

    [HttpDelete("{groupId}/members/{userId}")]
    public async Task<IActionResult> KickMember(int groupId, int userId)
    {
        var currentUserId = User.GetUserId();
        var success = await _groupService.KickMemberAsync(groupId, userId, currentUserId);
        if (!success)
        {
            return StatusCode(403, "You do not have permission to kick this member, or the member does not exist.");
        }
        return NoContent();
    }
}
