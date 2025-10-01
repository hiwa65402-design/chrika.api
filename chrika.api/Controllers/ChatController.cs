// Controllers/ChatController.cs

using Chrika.Api.DTOs;
using Chrika.Api.Helpers;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using System.Threading.Tasks;
using Chrika.Api.Models; 

namespace Chrika.Api.Controllers
{
    [Route("api/chat")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;


        public ChatController(IChatService chatService, IFileService fileService)
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
        //// === Endpointـی نوێ بۆ Upload کردنی فایلی چات ===
        //// POST: /api/chat/upload-file
        //[HttpPost("upload-file")]
        //public async Task<IActionResult> UploadChatFile([FromForm] IFormFile file, [FromForm] FileType fileType)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest("No file uploaded.");
        //    }

        //    // فایلەکە پاشەکەوت دەکەین و URLـەکەی وەردەگرین
        //    var fileUrl = await _fileService.SaveFileAsync(file, fileType);

        //    // تەنها URLـەکە دەگەڕێنینەوە
        //    return Ok(new { url = fileUrl });
        //}
    }
}
