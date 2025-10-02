// Controllers/GroupManagementController.cs

using Chrika.Api.DTOs;
using Chrika.Api.Helpers;
using Chrika.Api.Models;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Chrika.Api.Controllers
{
    [Route("api/groups")]
    [ApiController]
    [Authorize]
    public class GroupManagementController : ControllerBase
    {
        private readonly IGroupManagementService _groupManagementService;

        public GroupManagementController(IGroupManagementService groupManagementService)
        {
            _groupManagementService = groupManagementService;
        }

        // ١. دروستکردنی گروپ
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
        {
            var creatorId = User.GetUserId();
            var groupDto = await _groupManagementService.CreateGroupAsync(dto, creatorId);
            if (groupDto == null) return BadRequest(new { message = "Group username is already taken." });
            return CreatedAtAction(nameof(GetGroupDetails), new { groupId = groupDto.Id }, groupDto);
        }

        // ٢. هێنانی زانیاری گروپ
        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetGroupDetails(int groupId)
        {
            var group = await _groupManagementService.GetGroupDetailsAsync(groupId);
            if (group == null) return NotFound();
            return Ok(group);
        }

        // ٣. نوێکردنەوەی زانیاری گروپ
        [HttpPut("{groupId}")]
        public async Task<IActionResult> UpdateGroupInfo(int groupId, [FromBody] UpdateGroupDto dto)
        {
            var success = await _groupManagementService.UpdateGroupInfoAsync(groupId, dto, User.GetUserId());
            if (!success) return Forbid("You are not the owner of this group or the group does not exist.");
            return NoContent();
        }

        // ٤. سڕینەوەی گروپ
        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            var success = await _groupManagementService.DeleteGroupAsync(groupId, User.GetUserId());
            if (!success) return Forbid("You are not the owner of this group or the group does not exist.");
            return NoContent();
        }

        // ٥. هێنانی لیستی ئەندامان
        [HttpGet("{groupId}/members")]
        public async Task<IActionResult> GetGroupMembers(int groupId)
        {
            var members = await _groupManagementService.GetGroupMembersAsync(groupId);
            return Ok(members);
        }

        // ٦. زیادکردنی ئەندام
        [HttpPost("{groupId}/members/{userIdToAdd}")]
        public async Task<IActionResult> AddMember(int groupId, int userIdToAdd)
        {
            var success = await _groupManagementService.AddMemberAsync(groupId, userIdToAdd, User.GetUserId());
            if (!success) return Forbid("You do not have permission to add members to this group.");
            return Ok();
        }

        // ٧. لابردنی ئەندام
        [HttpDelete("{groupId}/members/{userIdToRemove}")]
        public async Task<IActionResult> RemoveMember(int groupId, int userIdToRemove)
        {
            var success = await _groupManagementService.RemoveMemberAsync(groupId, userIdToRemove, User.GetUserId());
            if (!success) return Forbid("You do not have permission to remove this member.");
            return Ok();
        }

        // ٨. خۆ دەرکردن لە گروپ
        [HttpPost("leave/{groupId}")]
        public async Task<IActionResult> LeaveGroup(int groupId)
        {
            var success = await _groupManagementService.LeaveGroupAsync(groupId, User.GetUserId());
            if (!success) return BadRequest("You cannot leave this group (perhaps you are the owner?).");
            return Ok();
        }

        // ٩. گۆڕینی ڕۆڵی ئەندام
        [HttpPut("{groupId}/members/{userIdToChange}/role")]
        public async Task<IActionResult> ChangeMemberRole(int groupId, int userIdToChange, [FromBody] GroupRole newRole)
        {
            var success = await _groupManagementService.ChangeMemberRoleAsync(groupId, userIdToChange, newRole, User.GetUserId());
            if (!success) return Forbid("Only the group owner can change roles.");
            return Ok();
        }
    }
}
