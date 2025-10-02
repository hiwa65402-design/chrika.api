// Controllers/GroupChatController.cs

using Chrika.Api.DTOs;
using Chrika.Api.Helpers; // بۆ User.GetUserId()
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Chrika.Api.Controllers
{
    [Route("api/group-chat")]
    [ApiController]
    [Authorize] // دڵنیابە کە بەکارهێنەر loginـی کردووە
    public class GroupChatController : ControllerBase
    {
        private readonly IGroupChatService _groupChatService;

        public GroupChatController(IGroupChatService groupChatService)
        {
            _groupChatService = groupChatService;
        }

        // === Endpointـی یەکەم: هێنانی لیستی گروپەکانم ===
        // GET: /api/group-chat/my-groups
        [HttpGet("my-groups")]
        public async Task<IActionResult> GetMyGroups()
        {
            var userId = User.GetUserId(); // وەرگرتنی IDی بەکارهێنەری ئێستا
            var groups = await _groupChatService.GetMyGroupsAsync(userId);
            return Ok(groups);
        }

        // === Endpointـی دووەم: هێنانی نامەکانی گروپ ===
        // GET: /api/group-chat/{groupId}/messages
        [HttpGet("{groupId}/messages")]
        public async Task<IActionResult> GetGroupMessages(int groupId)
        {
            var userId = User.GetUserId();
            var messages = await _groupChatService.GetGroupMessagesAsync(groupId, userId);

            if (messages == null)
            {
                // ئەمە مانای وایە بەکارهێنەر ئەندامی گروپەکە نییە
                return Forbid("You are not a member of this group.");
            }

            return Ok(messages);
        }

        // === Endpointـی سێیەم: ناردنی نامە بۆ گروپ ===
        // POST: /api/group-chat/{groupId}/messages
        [HttpPost("{groupId}/messages")]
        public async Task<IActionResult> SendMessageToGroup(int groupId, SendMessageDto dto)
        {
            var senderId = User.GetUserId();
            var messageDto = await _groupChatService.SendMessageToGroupAsync(groupId, senderId, dto);

            if (messageDto == null)
            {
                // ئەمە مانای وایە بەکارهێنەر ئەندامی گروپەکە نییە
                return Forbid("You cannot send messages to a group you are not a member of.");
            }

            // گەڕاندنەوەی نامە دروستکراوەکە
            return Ok(messageDto);
        }
    }
}
