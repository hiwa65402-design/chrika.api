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

        public async Task<CommentDto?> CreateCommentAsync(int entityId, string entityType, CreateCommentDto createCommentDto, int userId)
        {
            bool entityExists = entityType.ToLower() switch
            {
                "post" => await _context.Posts.AnyAsync(p => p.Id == entityId),
                "grouppost" => await _context.GroupPosts.AnyAsync(gp => gp.Id == entityId),
                _ => false
            };

            if (!entityExists) return null;

            var comment = new Comment
            {
                Content = createCommentDto.Content,
                UserId = userId,
                PostId = entityType.ToLower() == "post" ? entityId : null,
                GroupPostId = entityType.ToLower() == "grouppost" ? entityId : null
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            return MapToCommentDto(comment, user);
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsAsync(int entityId, string entityType)
        {
            IQueryable<Comment> query = _context.Comments.AsNoTracking();

            query = entityType.ToLower() switch
            {
                "post" => query.Where(c => c.PostId == entityId),
                "grouppost" => query.Where(c => c.GroupPostId == entityId),
                _ => query.Where(c => false)
            };

            return await query
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .Select(c => MapToCommentDto(c, c.User))
                .ToListAsync();
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null || comment.UserId != userId)
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
                Username = user.Username,
                UserProfilePicture = user.ProfilePicture
            };
        }
    }
}
