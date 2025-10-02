// Services/GroupChatService.cs

using Chrika.Api.Data;
using Chrika.Api.DTOs;
using Chrika.Api.Hubs;
using Chrika.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class GroupChatService : IGroupChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public GroupChatService(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // === فانکشنی یەکەم: هێنانی لیستی گروپەکانم ===
        public async Task<IEnumerable<GroupConversationListItemDto>> GetMyGroupsAsync(int userId)
        {
            var groups = await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                    .ThenInclude(g => g.Messages)
                .Select(gm => gm.Group)
                .ToListAsync();

            var groupListItems = new List<GroupConversationListItemDto>();

            foreach (var group in groups)
            {
                var lastMessage = group.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();

                groupListItems.Add(new GroupConversationListItemDto
                {
                    GroupId = group.Id,
                    GroupName = group.Name,
                    GroupProfilePicture = group.ProfilePictureUrl,
                    LastMessage = lastMessage?.Content ?? (lastMessage?.MediaUrl != null ? "[Media]" : "No messages yet."),
                    LastMessageAt = lastMessage?.SentAt ?? group.CreatedAt,
                    UnreadCount = 0 // بۆ ئێستا
                });
            }

            return groupListItems.OrderByDescending(g => g.LastMessageAt);
        }

        // === فانکشنی دووەم: هێنانی نامەکانی گروپ ===
        public async Task<IEnumerable<MessageDto>> GetGroupMessagesAsync(int groupId, int userId)
        {
            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

            if (!isMember)
            {
                return null;
            }

            var messages = await _context.Messages
                .Where(m => m.GroupId == groupId && !m.IsDeleted)
                .Include(m => m.Sender)
                .Include(m => m.ForwardedMessage)
                    .ThenInclude(fm => fm.Sender)
                .OrderBy(m => m.SentAt)
                .Select(m => MapToMessageDto(m))
                .ToListAsync();

            return messages;
        }

        // === فانکشنی سێیەم: ناردنی نامە بۆ گروپ ===
        public async Task<MessageDto> SendMessageToGroupAsync(int groupId, int senderId, SendMessageDto dto)
        {
            // ١. پشکنینی ئەوەی کە ئایا نێرەر ئەندامی گروپەکەیە
            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == senderId);

            if (!isMember)
            {
                return null; // یان فڕێدانی هەڵەیەک
            }

            // ٢. دروستکردنی ئۆبجێکتی نامەی نوێ
            var message = new Message
            {
                Content = dto.Content,
                MediaUrl = dto.MediaUrl,
                MediaDuration = dto.MediaDuration,
                Type = dto.Type,
                SentAt = DateTime.UtcNow,
                SenderId = senderId,
                GroupId = groupId, // گرنگ: نامەکە بە گروپەکەوە دەبەسترێتەوە
                ConversationId = null // گرنگ: ئەمە nullـە چونکە نامەکە بۆ گروپە
            };

            // ٣. پاشەکەوتکردنی نامەکە لە داتابەیس
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // ٤. هێنانەوەی نامەکە لەگەڵ زانیاری نێرەر
            var createdMessage = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.ForwardedMessage)
                    .ThenInclude(fm => fm.Sender)
                .FirstOrDefaultAsync(m => m.Id == message.Id);

            var messageDto = MapToMessageDto(createdMessage);

            // ٥. ناردنی نامەکە بە SignalR بۆ هەموو ئەندامانی گروپ
            string groupName = groupId.ToString();
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", messageDto);

            return messageDto;
        }

        // === فانکشنی یاریدەدەر ===
        private static MessageDto MapToMessageDto(Message message)
        {
            if (message == null) return null;

            return new MessageDto
            {
                Id = message.Id,
                Type = message.Type,
                Content = message.IsDeleted ? "This message was deleted." : message.Content,
                MediaUrl = message.IsDeleted ? null : message.MediaUrl,
                MediaDuration = message.IsDeleted ? null : message.MediaDuration,
                SentAt = message.SentAt,
                IsRead = message.IsRead,
                IsDeleted = message.IsDeleted,
                SenderId = message.SenderId,
                ForwardedMessage = MapToMessageDto(message.ForwardedMessage)
            };
        }
    }
}
