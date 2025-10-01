// Services/LikeService.cs

using Chrika.Api.Data;
using Chrika.Api.Dtos;
using Chrika.Api.DTOs; // بۆ NotificationDto
using Chrika.Api.Hubs;
using Chrika.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class LikeService : ILikeService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public LikeService(ApplicationDbContext context, INotificationService notificationService, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        public async Task<bool> ToggleLikeAsync(int? postId, int? groupPostId, int userId)
        {
            // 1. پشکنینی ئەوەی کە ئایا لایکەکە پێشتر بوونی هەیە
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && (l.PostId == postId || l.GroupPostId == groupPostId));

            if (existingLike != null)
            {
                // ئەگەر لایکەکە بوونی هەبوو، لای دەبەین (ئەنلایک)
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return true; // کردارەکە سەرکەوتووە
            }

            // 2. ئەگەر لایکەکە نوێ بوو، دروستی دەکەین
            var newLike = new Like
            {
                UserId = userId,
                PostId = postId,
                GroupPostId = groupPostId
            };
            _context.Likes.Add(newLike);

            // 3. دروستکردنی نۆتیفیکەیشن لە بنکەی دراوەدا
            Notification? createdNotification = null;
            if (postId.HasValue)
            {
                var post = await _context.Posts.FindAsync(postId.Value);
                if (post != null && post.UserId != userId) // دڵنیابە کەسێک نۆتیفیکەیشن بۆ خۆی دروست ناکات
                {
                    createdNotification = await _notificationService.CreateNotificationAsync(post.UserId, userId, NotificationType.NewLike, postId.Value);
                }
            }
            else if (groupPostId.HasValue)
            {
                var groupPost = await _context.GroupPosts.FindAsync(groupPostId.Value);
                if (groupPost != null && groupPost.AuthorId != userId)
                {
                    createdNotification = await _notificationService.CreateNotificationAsync(groupPost.AuthorId, userId, NotificationType.NewLike, groupPostId.Value);
                }
            }

            // 4. پاشەکەوتکردنی هەموو گۆڕانکارییەکان (لایک و نۆتیفیکەیشن)
            await _context.SaveChangesAsync();

            // 5. === ناردنی نۆتیفیکەیشنی ڕاستەوخۆ بە SignalR ===
            if (createdNotification != null)
            {
                // پێویستە DTOـی نۆتیفیکەیشنەکە دروست بکەین بۆ ناردن
                var user = await _context.Users.FindAsync(userId);
                var notificationDto = new NotificationDto
                {
                    Id = createdNotification.Id,
                    Message = _notificationService.GenerateNotificationMessage(createdNotification.Type, user.Username),
                    TriggeredByUsername = user.Username,
                    TriggeredByProfilePicture = user.ProfilePicture,
                    CreatedAt = createdNotification.CreatedAt,
                    IsRead = false,
                    EntityId = createdNotification.EntityId
                };

                // ناردنی DTOـی نۆتیفیکەیشنەکە بۆ گرووپی تایبەتی بەکارهێنەرەکە
                await _hubContext.Clients.Group(createdNotification.UserId.ToString())
                    .SendAsync("ReceiveNotification", notificationDto);
            }

            return true;
        }
    }
}
