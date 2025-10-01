using Chrika.Api.Data;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class LikeService : ILikeService
    {
        private readonly ApplicationDbContext _context;

        public LikeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleLikeAsync(int entityId, string entityType, int userId)
        {
            bool entityExists = entityType.ToLower() switch
            {
                "post" => await _context.Posts.AnyAsync(p => p.Id == entityId),
                "grouppost" => await _context.GroupPosts.AnyAsync(gp => gp.Id == entityId),
                _ => false
            };

            if (!entityExists) return false;

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l =>
                    (l.PostId == entityId && entityType.ToLower() == "post" && l.UserId == userId) ||
                    (l.GroupPostId == entityId && entityType.ToLower() == "grouppost" && l.UserId == userId));

            if (existingLike == null)
            {
                var newLike = new Like
                {
                    UserId = userId,
                    PostId = entityType.ToLower() == "post" ? entityId : null,
                    GroupPostId = entityType.ToLower() == "grouppost" ? entityId : null
                };
                _context.Likes.Add(newLike);
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
