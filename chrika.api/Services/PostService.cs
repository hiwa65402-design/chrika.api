using Chrika.Api.Data;
using Chrika.Api.DTOs;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore;
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

        public async Task<PostDto> CreatePostAsync(CreatePostDto createPostDto, int userId)
        {
            var post = new Post
            {
                Content = createPostDto.Content,
                ImageUrl = createPostDto.ImageUrl,
                UserId = userId, // دانانی خاوەنی پۆستەکە
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // بۆ ئەوەی PostDto بگەڕێنینەوە، پێویستە زانیاری User ـیشمان هەبێت
            var user = await _context.Users.FindAsync(userId);
            return MapToPostDto(post, user);
        }

        public async Task<IEnumerable<PostDto>> GetAllPostsAsync()
        {
            var posts = await _context.Posts
                .Include(p => p.User) // بۆ وەرگرتنی زانیاری خاوەنی پۆست
                .Include(p => p.Likes) // بۆ ژماردنی لایکەکان
                .Include(p => p.Comments) // بۆ ژماردنی کۆمێنتەکان
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return posts.Select(p => MapToPostDto(p, p.User));
        }

        // ... میتۆدەکانی تریش لێرە زیاد دەکەین ...

        // میتۆدێکی یارمەتیدەر بۆ گۆڕینی Post بۆ PostDto
        private static PostDto MapToPostDto(Post post, User user)
        {
            return new PostDto
            {
                Id = post.Id,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,
                Username = user.Username,
                LikesCount = post.Likes?.Count ?? 0,
                CommentsCount = post.Comments?.Count ?? 0
            };
        }

        // بۆ تەواوکردنی ئینتەرفەیسەکە، با ئەمانەش بە بەتاڵی دابنێین و دواتر پڕیان دەکەینەوە
        public Task<PostDto?> GetPostByIdAsync(int id)
        {
            throw new System.NotImplementedException();
        }
        public Task<PostDto?> UpdatePostAsync(int postId, UpdatePostDto updatePostDto, int userId)
        {
            throw new System.NotImplementedException();
        }
        public Task<bool> DeletePostAsync(int postId, int userId)
        {
            throw new System.NotImplementedException();
        }
    }
}
