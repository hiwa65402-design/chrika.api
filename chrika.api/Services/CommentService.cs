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

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CommentDto?> CreateCommentAsync(int postId, CreateCommentDto createCommentDto, int userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return null; // پۆستەکە بوونی نییە

            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Content = createCommentDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            return MapToCommentDto(comment, user);
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsForPostAsync(int postId)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .Select(c => MapToCommentDto(c, c.User))
                .ToListAsync();
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return false;

            // پشکنینی خاوەندارێتی: یان خاوەنی کۆمێنتەکە بیت، یان خاوەنی پۆستەکە بیت
            var post = await _context.Posts.FindAsync(comment.PostId);
            if (comment.UserId != userId && post.UserId != userId)
            {
                return false;
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        private static CommentDto MapToCommentDto(Comment comment, User user)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                Username = user.Username
            };
        }
    }
}
