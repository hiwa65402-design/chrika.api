// Controllers/ChatController.cs

using Chrika.Api.DTOs;
using Chrika.Api.Helpers;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Chrika.Api.Controllers
{
    [Route("api/chat")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // GET: /api/chat/conversations
        // لیستەی هەموو گفتوگۆکانت دەگەڕێنێتەوە
        [HttpGet("conversations")]
        public async Task<IActionResult> GetMyConversations()
        {
            var userId = User.GetUserId();
            var conversations = await _chatService.GetConversationsAsync(userId);
            return Ok(conversations);
        }

        // GET: /api/chat/conversations/{conversationId}/messages
        // هەموو نامەکانی ناو گفتوگۆیەک دەگەڕێنێتەوە
        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<IActionResult> GetConversationMessages(int conversationId)
        {
            var userId = User.GetUserId();
            var messages = await _chatService.GetMessagesAsync(conversationId, userId);
            if (messages == null)
            {
                return Forbid("You are not a participant of this conversation.");
            }
            return Ok(messages);
        }

        // POST: /api/chat/messages/user/{receiverId}
        // نامەیەک بۆ بەکارهێنەرێکی دیاریکراو دەنێرێت
        // ئەگەر گفتوگۆکە بوونی نەبێت، دروستی دەکات
        [HttpPost("messages/user/{receiverId}")]
        public async Task<IActionResult> SendMessageToUser(int receiverId, SendMessageDto dto)
        {
            var senderId = User.GetUserId();
            if (senderId == receiverId)
            {
                return BadRequest("You cannot send a message to yourself.");
            }

            var message = await _chatService.StartOrGetConversationAndSendMessageAsync(senderId, receiverId, dto);
            return Ok(message);
        }
    }
}
