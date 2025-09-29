using Chrika.Api.Data;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class LikeService : ILikeService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService; // <-- زیادکرا

        // constructor نوێکرایەوە
        public LikeService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService; // <-- زیادکرا
        }

        public async Task<bool> ToggleLikeAsync(int postId, int userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return false;

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike == null)
            {
                var like = new Like { PostId = postId, UserId = userId };
                _context.Likes.Add(like);

                // === لێرەدا ئاگادارکردنەوە دروست دەکەین ===
                await _notificationService.CreateNotificationAsync(post.UserId, userId, NotificationType.NewLike, postId);
            }
            else
            {
                _context.Likes.Remove(existingLike);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
