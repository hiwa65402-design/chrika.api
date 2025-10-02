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

        // Constructorـەکەت وەک خۆی دەمێنێتەوە
        public ChatController(IChatService chatService) // IFileService لادرا چونکە بەکارنەهاتبوو
        {
            _chatService = chatService;
        }

        // GET: /api/chat/conversations
        // لیستەی هەموو گفتوگۆکانت دەگەڕێنێتەوە
        [HttpGet("conversations")]
        public async Task<IActionResult> GetMyConversations()
        {
            var userId = User.GetUserId(); // وەرگرتنی IDی بەکارهێنەری لۆگین بوو
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
        [HttpPost("messages/user/{receiverId}")]
        public async Task<IActionResult> SendMessageToUser(int receiverId, [FromBody] SendMessageDto dto) // [FromBody] زیادکرا بۆ ڕوونی
        {
            var senderId = User.GetUserId();
            if (senderId == receiverId)
            {
                return BadRequest("You cannot send a message to yourself.");
            }

            var message = await _chatService.StartOrGetConversationAndSendMessageAsync(senderId, receiverId, dto);
            return Ok(message);
        }

        // ======================================================
        // ===            ئەم دوو Endpointـە زیادکراون            ===
        // ======================================================

        // DELETE: /api/chat/messages/{messageId}
        // Endpoint بۆ سڕینەوەی نامەیەک
        [HttpDelete("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var userId = User.GetUserId(); // وەرگرتنی IDی بەکارهێنەری ئێستا
            var success = await _chatService.DeleteMessageAsync(messageId, userId);

            if (!success)
            {
                // ئەگەر نامەکە بوونی نەبێت یان هی ئەم بەکارهێنەرە نەبێت
                return Forbid("You do not have permission to delete this message.");
            }

            // 204 NoContent واتە کردارەکە سەرکەوتوو بوو بەڵام هیچ داتایەک ناگەڕێتەوە
            return NoContent();
        }

        // POST: /api/chat/messages/{messageId}/forward/user/{receiverId}
        // Endpoint بۆ ئاڕاستەکردن (Forward)ی نامەیەک
        [HttpPost("messages/{messageId}/forward/user/{receiverId}")]
        public async Task<IActionResult> ForwardMessage(int messageId, int receiverId)
        {
            var forwarderId = User.GetUserId(); // ئەو کەسەی نامەکە forward دەکات

            if (forwarderId == receiverId)
            {
                return BadRequest("You cannot forward a message to yourself.");
            }

            var forwardedMessage = await _chatService.ForwardMessageAsync(messageId, forwarderId, receiverId);

            if (forwardedMessage == null)
            {
                // ئەگەر نامە ڕەسەنەکە نەدۆزرایەوە
                return NotFound("The original message to forward was not found.");
            }

            // 200 OK لەگەڵ زانیاری نامە forward کراوەکە
            return Ok(forwardedMessage);
        }
    }
}
