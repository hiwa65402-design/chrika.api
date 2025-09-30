using Chrika.Api.Data;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class LikeService : ILikeService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IAnalyticsService _analyticsService;

        public LikeService(ApplicationDbContext context, INotificationService notificationService, IAnalyticsService analyticsService)
        {
            _context = context;
            _notificationService = notificationService;
            _analyticsService = analyticsService;
        }

        public async Task<bool> ToggleLikeAsync(int postId, int userId)
        {
            // === زیادکرا: پشکنینی بوونی پۆست و بەکارهێنەر ===
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId) || await _context.PagePosts.AnyAsync(pp => pp.Id == postId);
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);

            // ئەگەر یەکێکیان بوونی نەبوو، فانکشنەکە دەوەستێت و هەڵە ڕوونادات
            if (!postExists || !userExists)
            {
                return false;
            }

            var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike == null)
            {
                var like = new Like { PostId = postId, UserId = userId };
                _context.Likes.Add(like);

                var userPost = await _context.Posts.FindAsync(postId);
                if (userPost != null)
                {
                    await _notificationService.CreateNotificationAsync(userPost.UserId, userId, NotificationType.NewLike, postId);
                }

                await CheckAndRecordInteraction(postId, InteractionType.Like, userId);
            }
            else
            {
                _context.Likes.Remove(existingLike);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task CheckAndRecordInteraction(int postId, InteractionType type, int userId)
        {
            var pagePost = await _context.PagePosts.FirstOrDefaultAsync(pp => pp.Id == postId);
            if (pagePost != null)
            {
                var activeCampaign = await _context.AdCampaigns
                    .FirstOrDefaultAsync(c => c.PagePostId == postId && c.Status == CampaignStatus.Active && c.EndDate > DateTime.UtcNow);

                if (activeCampaign != null)
                {
                    await _analyticsService.RecordInteractionAsync(activeCampaign.Id, type, userId);
                }
            }
        }
    }
}
