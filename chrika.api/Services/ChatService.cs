// Services/ChatService.cs

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
        private readonly IHubContext<NotificationHub> _hubContext;

        public ChatService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // === 1. هێنانی لیستەی هەموو گفتوگۆکان ===
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
                    // زانیاری ئەو کەسەی کە قسەی لەگەڵ دەکەیت
                    OtherUserId = c.Participant1Id == userId ? c.Participant2Id : c.Participant1Id,
                    OtherUsername = c.Participant1Id == userId ? c.Participant2.Username : c.Participant1.Username,
                    OtherUserProfilePicture = c.Participant1Id == userId ? c.Participant2.ProfilePicture : c.Participant1.ProfilePicture,
                    // زانیاری دوایین نامە
                    LastMessage = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault().Content ?? "[Media]",
                    LastMessageAt = c.LastMessageAt,
                    // ژمارەی نامە نەخوێندراوەکان
                    UnreadCount = c.Messages.Count(m => !m.IsRead && m.SenderId != userId)
                })
                .ToListAsync();
        }

        // === 2. هێنانی هەموو نامەکانی ناو یەک گفتوگۆ ===
        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, int userId)
        {
            // دڵنیابوونەوە لەوەی کە بەکارهێنەر ئەندامی ئەم گفتوگۆیە
            var isParticipant = await _context.Conversations
                .AnyAsync(c => c.Id == conversationId && (c.Participant1Id == userId || c.Participant2Id == userId));

            if (!isParticipant)
            {
                return null; // یان exception
            }

            // هێنانی نامەکان
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .Select(m => MapToMessageDto(m)) // فانکشنێکی یاریدەدەر
                .ToListAsync();

            // نیشانکردنی نامەکان وەک خوێندراوە
            var unreadMessages = await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }
            await _context.SaveChangesAsync();

            return messages;
        }

        // === 3. ناردنی نامە بۆ گفتوگۆیەکی پێشتر دروستکراو ===
        public async Task<MessageDto> SendMessageAsync(int conversationId, int senderId, SendMessageDto dto)
        {
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null) return null;

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = dto.Content,
                Type = MessageType.Text
            };

            // نوێکردنەوەی کاتی دوایین نامە
            conversation.LastMessageAt = message.SentAt;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var messageDto = MapToMessageDto(message);

            // ناردنی نامەکە بە SignalR بۆ وەرگرەکە
            var receiverId = conversation.Participant1Id == senderId ? conversation.Participant2Id : conversation.Participant1Id;
            await _hubContext.Clients.Group(receiverId.ToString())
                .SendAsync("ReceiveMessage", messageDto);

            return messageDto;
        }

        // === 4. دەستپێکردنی گفتوگۆ و ناردنی یەکەم نامە ===
        public async Task<MessageDto> StartOrGetConversationAndSendMessageAsync(int senderId, int receiverId, SendMessageDto dto)
        {
            // پشکنینی ئەوەی کە ئایا گفتوگۆکە پێشتر بوونی هەیە
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => (c.Participant1Id == senderId && c.Participant2Id == receiverId) ||
                                          (c.Participant1Id == receiverId && c.Participant2Id == senderId));

            if (conversation == null)
            {
                // ئەگەر بوونی نەبوو، دانەیەکی نوێ دروست دەکەین
                conversation = new Conversation
                {
                    Participant1Id = senderId,
                    Participant2Id = receiverId
                };
                _context.Conversations.Add(conversation);
            }

            // ناردنی نامەکە بە بەکارهێنانی فانکشنەکەی سەرەوە
            return await SendMessageAsync(conversation.Id, senderId, dto);
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
                ForwardedMessage = MapToMessageDto(message.ForwardedMessage) // Recursive call
            };
        }
    }
}
