using Chrika.Api.Data;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class FollowService : IFollowService
    {
        private readonly ApplicationDbContext _context;

        public FollowService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleFollowAsync(int followerId, int followingId)
        {
            // نابێت کەسێک خۆی فۆڵۆ بکات
            if (followerId == followingId)
            {
                return false;
            }

            // پشکنین بکە بزانە ئەو بەکارهێنەرەی کە دەتەوێت فۆڵۆی بکەیت بوونی هەیە
            var userToFollowExists = await _context.Users.AnyAsync(u => u.Id == followingId);
            if (!userToFollowExists)
            {
                return false;
            }

            // بزانە ئایا پێشتر فۆڵۆت کردووە
            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (existingFollow == null)
            {
                // ئەگەر فۆڵۆت نەکردبوو، پەیوەندییەکی نوێی فۆڵۆ دروست بکە
                var follow = new Follow { FollowerId = followerId, FollowingId = followingId };
                _context.Follows.Add(follow);
            }
            else
            {
                // ئەگەر فۆڵۆت کردبوو، پەیوەندییەکە بسڕەوە
                _context.Follows.Remove(existingFollow);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
