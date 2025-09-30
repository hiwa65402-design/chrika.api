using Chrika.Api.Data;
using Chrika.Api.DTOs;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IAnalyticsService _analyticsService; // <-- زیادکرا

        // === constructor نوێکرایەوە بۆ وەرگرتنی هەرسێ سێرڤسەکە ===
        public CommentService(ApplicationDbContext context, INotificationService notificationService, IAnalyticsService analyticsService)
        {
            _context = context;
            _notificationService = notificationService;
            _analyticsService = analyticsService; // <-- زیادکرا
        }

        public async Task<CommentDto?> CreateCommentAsync(int postId, CreateCommentDto createCommentDto, int userId)
        {
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId) || await _context.PagePosts.AnyAsync(pp => pp.Id == postId);
            if (!postExists) return null;

            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Content = createCommentDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);

            // تۆمارکردنی ئاگادارکردنەوە (تەنها بۆ پۆستی ئاسایی)
            var userPost = await _context.Posts.FindAsync(postId);
            if (userPost != null)
            {
                await _notificationService.CreateNotificationAsync(userPost.UserId, userId, NotificationType.NewComment, postId);
            }

            // === زیادکرا: تۆمارکردنی ئامار ===
            await CheckAndRecordInteraction(postId, InteractionType.Comment, userId);

            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            return MapToCommentDto(comment, user);
        }

        // ... (باقی میتۆدەکانی تر وەک خۆیان) ...
        public async Task<IEnumerable<CommentDto>> GetCommentsForPostAsync(int postId)
        {
            return await _context.Comments.Where(c => c.PostId == postId).Include(c => c.User).OrderBy(c => c.CreatedAt).Select(c => MapToCommentDto(c, c.User)).ToListAsync();
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return false;

            var post = await _context.Posts.FindAsync(comment.PostId);
            if (post != null && comment.UserId != userId && post.UserId != userId) return false;

            var pagePost = await _context.PagePosts.Include(pp => pp.Page).FirstOrDefaultAsync(pp => pp.Id == comment.PostId);
            if (pagePost != null && comment.UserId != userId && pagePost.Page.OwnerId != userId) return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        private static CommentDto MapToCommentDto(Comment comment, User user)
        {
            return new CommentDto { Id = comment.Id, Content = comment.Content, CreatedAt = comment.CreatedAt, Username = user.Username };
        }

        // === زیادکرا: هەمان میتۆدی یاریدەدەر ===
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
