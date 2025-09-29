using Chrika.Api.Data;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class FollowService : IFollowService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService; // <-- زیادکرا

        // constructor نوێکرایەوە
        public FollowService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService; // <-- زیادکرا
        }

        public async Task<bool> ToggleFollowAsync(int followerId, int followingId)
        {
            if (followerId == followingId) return false;

            var userToFollowExists = await _context.Users.AnyAsync(u => u.Id == followingId);
            if (!userToFollowExists) return false;

            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (existingFollow == null)
            {
                var follow = new Follow { FollowerId = followerId, FollowingId = followingId };
                _context.Follows.Add(follow);

                // === لێرەدا ئاگادارکردنەوە دروست دەکەین ===
                await _notificationService.CreateNotificationAsync(followingId, followerId, NotificationType.NewFollower, null);
            }
            else
            {
                _context.Follows.Remove(existingFollow);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
