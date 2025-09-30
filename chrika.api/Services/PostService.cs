using Chrika.Api.Data;
using Chrika.Api.DTOs;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;

        public PostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PostDto>> GetAllPostsAsync(int? currentUserId = null)
        {
            return await _context.Posts
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    UserProfilePicture = p.User.ProfilePicture,
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count,
                    SharesCount = _context.Shares.Count(s => s.PostId == p.Id),
                    IsLikedByCurrentUser = currentUserId.HasValue && p.Likes.Any(l => l.UserId == currentUserId.Value),
                    IsSharedByCurrentUser = currentUserId.HasValue && _context.Shares.Any(s => s.PostId == p.Id && s.UserId == currentUserId.Value)
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PostDto>> GetFeedForUserAsync(int userId)
        {
            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync();
            followingIds.Add(userId);

            return await _context.Posts
                .Where(p => followingIds.Contains(p.UserId))
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    UserProfilePicture = p.User.ProfilePicture,
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count,
                    SharesCount = _context.Shares.Count(s => s.PostId == p.Id),
                    IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == userId),
                    IsSharedByCurrentUser = _context.Shares.Any(s => s.PostId == p.Id && s.UserId == userId)
                })
                .ToListAsync();
        }

        public async Task<PostDto?> GetPostByIdAsync(int id, int? currentUserId = null)
        {
            return await _context.Posts
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    UserProfilePicture = p.User.ProfilePicture,
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count,
                    SharesCount = _context.Shares.Count(s => s.PostId == p.Id),
                    IsLikedByCurrentUser = currentUserId.HasValue && p.Likes.Any(l => l.UserId == currentUserId.Value),
                    IsSharedByCurrentUser = currentUserId.HasValue && _context.Shares.Any(s => s.PostId == p.Id && s.UserId == currentUserId.Value)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PostDto> CreatePostAsync(CreatePostDto createPostDto, int userId)
        {
            var post = new Post
            {
                Content = createPostDto.Content,
                ImageUrl = createPostDto.ImageUrl,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            return new PostDto
            {
                Id = post.Id,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,
                UserId = user.Id,
                Username = user.Username,
                UserProfilePicture = user.ProfilePicture,
                LikesCount = 0,
                CommentsCount = 0,
                SharesCount = 0,
                IsLikedByCurrentUser = false,
                IsSharedByCurrentUser = false
            };
        }

        public async Task<PostDto?> UpdatePostAsync(int postId, UpdatePostDto updatePostDto, int userId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null) return null;
            if (post.UserId != userId) return null;

            post.Content = updatePostDto.Content;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetPostByIdAsync(postId, userId);
        }

        public async Task<bool> DeletePostAsync(int postId, int userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return false;
            if (post.UserId != userId) return false;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
