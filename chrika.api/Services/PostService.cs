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

        // === گۆڕانکاری لێرەدا کراوە ===
        public async Task<IEnumerable<PostDto>> GetAllPostsAsync(int? currentUserId = null)
        {
            return await _context.Posts
                .AsNoTracking() // بۆ خێراکردنی خوێندنەوە
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
                    // === لێرەدا زیادکرا ===
                    SharesCount = _context.Shares.Count(s => s.PostId == p.Id),
                    IsLikedByCurrentUser = currentUserId.HasValue && p.Likes.Any(l => l.UserId == currentUserId.Value)
                })
                .ToListAsync();
        }

        // === گۆڕانکاری لێرەدا کراوە ===
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
                    // === لێرەدا زیادکرا ===
                    SharesCount = _context.Shares.Count(s => s.PostId == p.Id),
                    IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == userId)
                })
                .ToListAsync();
        }

        // فانکشنەکانی تر وەک خۆیان دەمێننەوە، بەڵام با MapToPostDto لاببەین چونکە ئیتر پێویست نییە
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

            // دوای دروستکردن، پۆستە نوێیەکە وەک DTO دەگەڕێنینەوە
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
                SharesCount = 0, // پۆستی نوێ هیچ شێرێکی نییە
                IsLikedByCurrentUser = false
            };
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
                    IsLikedByCurrentUser = currentUserId.HasValue && p.Likes.Any(l => l.UserId == currentUserId.Value)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PostDto?> UpdatePostAsync(int postId, UpdatePostDto updatePostDto, int userId)
        {
            var post = await _context.Posts.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null) return null;
            if (post.UserId != userId) return null;

            post.Content = updatePostDto.Content;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetPostByIdAsync(postId, userId); // فانکشنەکەی سەرەوە بانگ دەکەینەوە بۆ گەڕاندنەوەی داتای نوێ
        }
    }
}
