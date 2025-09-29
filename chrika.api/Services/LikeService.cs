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

        public async Task<bool> ToggleLikeAsync(int postId, int userId)
        {
            // پشکنین بکە بزانە پۆستەکە بوونی هەیە
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
            {
                return false; // پۆستەکە بوونی نییە
            }

            // بزانە ئایا بەکارهێنەرەکە پێشتر ئەم پۆستەی لایک کردووە
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike == null)
            {
                // ئەگەر لایکی نەکردبوو، لایکێکی نوێ زیاد بکە
                var like = new Like { PostId = postId, UserId = userId };
                _context.Likes.Add(like);
            }
            else
            {
                // ئەگەر لایکی کردبوو، لایکەکە بسڕەوە
                _context.Likes.Remove(existingLike);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
