// DTOs/ChatDtos.cs
using System;
using Chrika.Api.Models; // بۆ MessageType

namespace Chrika.Api.DTOs
{
    // بۆ پیشاندانی لیستەی گفتوگۆکان
    public class ConversationListItemDto
    {
        public int ConversationId { get; set; }
        public int OtherUserId { get; set; }
        public string? OtherUsername { get; set; }
        public string? OtherUserProfilePicture { get; set; }
        public string? LastMessage { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }

    // بۆ پیشاندانی نامەکانی ناو گفتوگۆیەک
    public class MessageDto
    {
        public int Id { get; set; }
        public MessageType Type { get; set; }
        public string? Content { get; set; }
        public string? MediaUrl { get; set; }
        public double? MediaDuration { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
        public int SenderId { get; set; }

        // بۆ نامەی forward کراو
        public MessageDto? ForwardedMessage { get; set; }
    }

    // بۆ ناردنی نامەی نوێ
    public class SendMessageDto
    {
        public string? Content { get; set; }
    }
}
