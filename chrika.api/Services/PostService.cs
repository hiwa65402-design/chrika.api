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
        public async Task<bool> DeletePostAsync(int postId, int userId)
        {
            // 1. پۆستەکە لە داتابەیس بدۆزەرەوە
            var post = await _context.Posts.FindAsync(postId);

            // 2. ئەگەر پۆستەکە بوونی نەبوو، false بگەڕێنەرەوە
            if (post == null)
            {
                return false;
            }

            // 3. === گرنگترین بەش: پشکنینی خاوەندارێتی ===
            //    ئایا ئەو بەکارهێنەرەی داواکارییەکەی ناردووە (userId)
            //    هەمان خاوەنی پۆستەکەیە (post.UserId)؟
            if (post.UserId != userId)
            {
                // ئەگەر خاوەنی نەبوو، ڕێگەی پێنادەین بیسڕێتەوە
                // لێرەدا false دەگەڕێنینەوە، لە کۆنترۆڵەر وەڵامی 403 Forbidden دەنێرین
                return false;
            }

            // 4. ئەگەر هەموو شتێک ڕاست بوو، پۆستەکە بسڕەوە
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return true;
        }

        // بۆ تەواوکردنی ئینتەرفەیسەکە، با ئەمانەش بە بەتاڵی دابنێین و دواتر پڕیان دەکەینەوە
        public Task<PostDto?> GetPostByIdAsync(int id)
        {
            throw new System.NotImplementedException();
        }
        public async Task<PostDto?> UpdatePostAsync(int postId, UpdatePostDto updatePostDto, int userId)
        {
            // 1. پۆستەکە لە داتابەیس بدۆزەرەوە
            var post = await _context.Posts
                                     .Include(p => p.User) // بۆ گەڕاندنەوەی PostDto پێویستمانە
                                     .FirstOrDefaultAsync(p => p.Id == postId);

            // 2. ئەگەر پۆستەکە بوونی نەبوو، null بگەڕێنەرەوە
            if (post == null)
            {
                return null;
            }

            // 3. پشکنینی خاوەندارێتی
            if (post.UserId != userId)
            {
                return null; // یان دەتوانین exceptionـێک هەڵبدەین
            }

            // 4. نوێکردنەوەی ناوەڕۆکی پۆستەکە
            post.Content = updatePostDto.Content;
            post.UpdatedAt = DateTime.UtcNow;

            // 5. خەزنکردنی گۆڕانکارییەکان
            await _context.SaveChangesAsync();

            // 6. گەڕاندنەوەی پۆستە نوێکراوەکە وەک DTO
            return new PostDto
            {
                Id = post.Id,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,
                Username = post.User.Username,
                LikesCount = post.Likes?.Count() ?? 0,
                CommentsCount = post.Comments?.Count() ?? 0
            };
        }

        public async Task<IEnumerable<PostDto>> GetFeedForUserAsync(int userId)
        {
            // 1. لیستی IDی ئەو کەسانە وەربگرە کە بەکارهێنەرەکە فۆڵۆی کردوون
            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            // 2. IDی بەکارهێنەرەکە خۆشی بخەرە ناو لیستەکەوە بۆ ئەوەی پۆستەکانی خۆشی ببینێت
            followingIds.Add(userId);

            // 3. هەموو ئەو پۆستانە بهێنەرەوە کە UserId ـیان لەناو لیستی `followingIds` دایە
            var feedPosts = await _context.Posts
                .Where(p => followingIds.Contains(p.UserId)) // گرنگترین بەش
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostDto // ڕاستەوخۆ لێرەدا دەیگۆڕین بۆ DTO
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    Username = p.User.Username,
                    LikesCount = p.Likes.Count,
                    CommentsCount = p.Comments.Count
                })
                .ToListAsync();

            return feedPosts;
        }

    }
}
