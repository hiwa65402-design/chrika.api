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
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;
        // گۆڕیم بۆ ChatHub وەک لە کۆدە سەرەتاییەکاندا بوو
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatService(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // === ١. هێنانی لیستەی هەموو گفتوگۆکان ===
        public async Task<IEnumerable<ConversationListItemDto>> GetConversationsAsync(int userId)
        {
            return await _context.Conversations
                .Where(c => c.Participant1Id == userId || c.Participant2Id == userId)
                .Include(c => c.Participant1)
                .Include(c => c.Participant2)
                .Include(c => c.Messages)
                .OrderByDescending(c => c.LastMessageAt)
                .Select(c => new ConversationListItemDto
                {
                    ConversationId = c.Id,
                    OtherUserId = c.Participant1Id == userId ? c.Participant2Id : c.Participant1Id,
                    OtherUsername = c.Participant1Id == userId ? c.Participant2.Username : c.Participant1.Username,
                    OtherUserProfilePicture = c.Participant1Id == userId ? c.Participant2.ProfilePicture : c.Participant1.ProfilePicture,
                    LastMessage = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault().Content ?? "[Media]",
                    LastMessageAt = c.LastMessageAt,
                    UnreadCount = c.Messages.Count(m => !m.IsRead && m.SenderId != userId)
                })
                .ToListAsync();
        }

        // === ٢. هێنانی هەموو نامەکانی ناو یەک گفتوگۆ ===
        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, int userId)
        {
            var isParticipant = await _context.Conversations
                .AnyAsync(c => c.Id == conversationId && (c.Participant1Id == userId || c.Participant2Id == userId));

            if (!isParticipant) return null;

            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .Include(m => m.ForwardedMessage).ThenInclude(fm => fm.Sender) // بۆ هێنانی زانیاری نامەی forward کراو
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var unreadMessages = messages.Where(m => m.SenderId != userId && !m.IsRead).ToList();
            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }

            return messages.Select(MapToMessageDto);
        }

        // === ٣. ناردنی نامە ===
        public async Task<MessageDto> SendMessageAsync(int conversationId, int senderId, SendMessageDto dto)
        {
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null) return null;

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Type = dto.Type,
                Content = dto.Content,
                MediaUrl = dto.MediaUrl,
                MediaDuration = dto.MediaDuration
            };

            conversation.LastMessageAt = message.SentAt;
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var messageDto = MapToMessageDto(message);
            var receiverId = conversation.Participant1Id == senderId ? conversation.Participant2Id : conversation.Participant1Id;

            // ناردنی ئاگاداری بۆ وەرگر
            await _hubContext.Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", messageDto);

            return messageDto;
        }

        // === ٤. دەستپێکردنی گفتوگۆ و ناردنی یەکەم نامە ===
        public async Task<MessageDto> StartOrGetConversationAndSendMessageAsync(int senderId, int receiverId, SendMessageDto dto)
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => (c.Participant1Id == senderId && c.Participant2Id == receiverId) ||
                                          (c.Participant1Id == receiverId && c.Participant2Id == senderId));

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    Participant1Id = senderId,
                    Participant2Id = receiverId,
                };
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            return await SendMessageAsync(conversation.Id, senderId, dto);
        }

        // === ٥. سڕینەوەی نامە (تەواوکراو) ===
        public async Task<bool> DeleteMessageAsync(int messageId, int userId)
        {
            var message = await _context.Messages
                                .Include(m => m.Conversation)
                                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null || message.SenderId != userId)
            {
                return false;
            }

            message.IsDeleted = true;
            message.Content = null; // ناوەڕۆک و میدیاکەی لادەبەین
            message.MediaUrl = null;
            message.MediaDuration = null;

            await _context.SaveChangesAsync();

            // ئاگادارکردنەوەی هەردوو لایەنی گفتوگۆکە
            var receiverId = message.Conversation.Participant1Id == userId ? message.Conversation.Participant2Id : message.Conversation.Participant1Id;
            await _hubContext.Clients.User(userId.ToString()).SendAsync("MessageDeleted", new { messageId });
            await _hubContext.Clients.User(receiverId.ToString()).SendAsync("MessageDeleted", new { messageId });

            return true;
        }

        // === ٦. ئاڕاستەکردنی (Forward) نامە (تەواوکراو) ===
        public async Task<MessageDto?> ForwardMessageAsync(int messageId, int forwarderId, int receiverId)
        {
            var originalMessage = await _context.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == messageId);
            if (originalMessage == null) return null;

            var dto = new SendMessageDto
            {
                Type = MessageType.Forwarded, // جۆری نامەکە دەگۆڕین
                Content = originalMessage.Content,
                MediaUrl = originalMessage.MediaUrl,
                MediaDuration = originalMessage.MediaDuration,
            };

            // بەکارهێنانی فانکشنی پێشوو بۆ دروستکردنی گفتوگۆ و ناردنی نامە
            var forwardedMessageDto = await StartOrGetConversationAndSendMessageAsync(forwarderId, receiverId, dto);

            // دۆزینەوەی نامە تازەکە و زیادکردنی ForwardedMessageId
            var newMessage = await _context.Messages.FindAsync(forwardedMessageDto.Id);
            if (newMessage != null)
            {
                newMessage.ForwardedMessageId = originalMessage.Id;
                await _context.SaveChangesAsync();
            }

            // هێنانی داتای تەواو بۆ ناردنەوە
            var finalMessage = await _context.Messages
                .Include(m => m.ForwardedMessage).ThenInclude(fm => fm.Sender)
                .FirstOrDefaultAsync(m => m.Id == newMessage.Id);

            return MapToMessageDto(finalMessage);
        }

        // فانکشنێکی یاریدەدەر بۆ گۆڕینی Message بۆ MessageDto
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
                // بەکارهێنانی Recursive call بۆ دروستکردنی DTO بۆ نامەی forward کراو
                ForwardedMessage = message.ForwardedMessage != null ? MapToMessageDto(message.ForwardedMessage) : null
            };
        }
    }
}
