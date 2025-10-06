// Services/PostService.cs

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
        private readonly IAnalyticsService _analyticsService;

        public PostService(ApplicationDbContext context, IAnalyticsService analyticsService)
        {
            _context = context;
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// فانکشنی نوێ و گشتگیر بۆ دروستکردنی Feed.
        /// پۆستی کەسەکان، گرووپەکان، و ڕیکلامەکان تێکەڵ دەکات.
        /// </summary>
     

        public async Task<IEnumerable<FeedItemDto>> GetUniversalFeedAsync(int? userId)
        {
            var feedItems = new List<FeedItemDto>();
            string? userLocation = null;

            if (userId.HasValue)
            {
                var user = await _context.Users.FindAsync(userId.Value);
                userLocation = user?.Location;
            }

            // --- 1. هێنانی پۆستی کەسەکان (ئەگەر لۆگین بوو) ---
            if (userId.HasValue)
            {
                var followingIds = await _context.Follows.Where(f => f.FollowerId == userId.Value).Select(f => f.FollowingId).ToListAsync();
                var userPosts = await _context.Posts
                    .Where(p => followingIds.Contains(p.UserId))
                    .Include(p => p.User)
                    .Select(p => new FeedItemDto
                    {
                        Id = p.Id,
                        ItemType = "Post",
                        Content = p.Content,
                        CreatedAt = p.CreatedAt,
                        AuthorId = p.UserId,
                        AuthorUsername = p.User.Username,
                        AuthorProfilePicture = p.User.ProfilePicture,
                        PostType = !string.IsNullOrEmpty(p.ImageUrl) ? "Image" : "Text",
                        MediaUrl = p.ImageUrl,
                        LikesCount = _context.Likes.Count(l => l.PostId == p.Id),
                        CommentsCount = _context.Comments.Count(c => c.PostId == p.Id),
                        IsLikedByCurrentUser = _context.Likes.Any(l => l.PostId == p.Id && l.UserId == userId.Value)
                    })
                    .ToListAsync();
                feedItems.AddRange(userPosts);
            }

            // --- 2. هێنانی پۆستی گرووپەکان ---
            var memberGroupIds = new List<int>();
            if (userId.HasValue)
            {
                memberGroupIds = await _context.GroupMembers.Where(m => m.UserId == userId.Value).Select(m => m.GroupId).ToListAsync();
            }
            var groupPosts = await _context.GroupPosts
                .Where(gp => memberGroupIds.Contains(gp.GroupId) || gp.Group.Type == GroupType.Public)
                .Include(gp => gp.Author).Include(gp => gp.Group)
                .Select(gp => new FeedItemDto
                {
                    Id = gp.Id,
                    ItemType = "GroupPost",
                    Content = gp.Content,
                    CreatedAt = gp.CreatedAt,
                    AuthorId = gp.AuthorId,
                    AuthorUsername = gp.Author.Username,
                    AuthorProfilePicture = gp.Author.ProfilePicture,
                    GroupId = gp.GroupId,
                    GroupName = gp.Group.Name,
                    GroupUsername = gp.Group.Username,
                    PostType = gp.PostType.ToString(),
                    MediaUrl = gp.MediaUrl,
                    LikesCount = _context.Likes.Count(l => l.GroupPostId == gp.Id),
                    CommentsCount = _context.Comments.Count(c => c.GroupPostId == gp.Id),
                    IsLikedByCurrentUser = userId.HasValue && _context.Likes.Any(l => l.GroupPostId == gp.Id && l.UserId == userId.Value)
                })
                .ToListAsync();
            feedItems.AddRange(groupPosts);

            // --- 3. هێنانی ڕیکلامەکان ---
            var allActiveCampaigns = await _context.AdCampaigns
                .Where(c => c.Status == CampaignStatus.Active && c.EndDate > System.DateTime.UtcNow)
                .Include(c => c.PagePost.Page)
                .ToListAsync();

            var relevantCampaigns = allActiveCampaigns
                .Where(c => c.Audience.Locations.Count == 0 || (userLocation != null && c.Audience.Locations.Contains(userLocation)))
                .ToList();

            var adItems = relevantCampaigns.Select(c => new FeedItemDto
            {
                Id = c.PagePost.Id,
                ItemType = "Ad",
                Content = c.PagePost.Content,
                CreatedAt = c.PagePost.CreatedAt,
                AuthorId = c.PagePost.Page.OwnerId,
                AuthorUsername = c.PagePost.Page.Name,
                AuthorProfilePicture = c.PagePost.Page.ProfilePicture,
                MediaUrl = c.PagePost.ImageUrl,
                IsSponsored = true,
                AdCampaignId = c.Id
            }).ToList();

            if (userId.HasValue)
            {
                foreach (var campaign in relevantCampaigns)
                {
                    await _analyticsService.RecordInteractionAsync(campaign.Id, InteractionType.Impression, userId.Value);
                }
            }

            // --- 4. تێکەڵکردنی هەموو شتەکان ---
            var combinedFeed = feedItems
                .GroupBy(item => new { item.Id, item.ItemType })
                .Select(g => g.First())
                .ToList(); // لێرەدا ToList() دەکەین بۆ ئەوەی بتوانین نمرە حیساب بکەین

            // === گۆڕانکارییەکە لێرەدایە: حیسابکردنی نمرە ===
            foreach (var item in combinedFeed)
            {
                var hoursAgo = (DateTime.UtcNow - item.CreatedAt).TotalHours;
                item.Score = (item.LikesCount * 2) + (item.CommentsCount * 3) - hoursAgo;
            }

            // === ڕیزکردنی نوێ: بەپێی نمرە ===
            combinedFeed = combinedFeed.OrderByDescending(item => item.Score).ToList();

            // دانانی ڕیکلامەکان (وەک پێشوو)
            int adIndex = 0;
            for (int i = 5; i < combinedFeed.Count; i += 6)
            {
                if (adIndex < adItems.Count)
                {
                    combinedFeed.Insert(i, adItems[adIndex]);
                    adIndex++;
                }
            }
            while (adIndex < adItems.Count)
            {
                combinedFeed.Add(adItems[adIndex]);
                adIndex++;
            }

            return combinedFeed;
        }
        // =================================================================
        // === فانکشنە کۆنەکان کە هێشتا پێویستن بۆ کارکردنی PostsController ===
        // =================================================================

        public async Task<IEnumerable<PostDto>> GetAllPostsAsync(int? userId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => MapToPostDto(p, userId)) // بەکارهێنانی فانکشنە یاریدەدەرەکە
                .ToListAsync();
        }

        public async Task<PostDto> GetPostByIdAsync(int id, int? userId)
        {
            var post = await _context.Posts
                .Where(p => p.Id == id)
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync();

            return MapToPostDto(post, userId); // بەکارهێنانی فانکشنە یاریدەدەرەکە
        }
        public async Task<PostDto> CreatePostAsync(CreatePostDto createPostDto, int userId)
        {
            var post = new Post
            {
                Content = createPostDto.Content,
                ImageUrl = createPostDto.ImageUrl,
                UserId = userId
            };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return (await GetPostByIdAsync(post.Id, userId))!;
        }

        public async Task<PostDto?> UpdatePostAsync(int postId, UpdatePostDto updatePostDto, int userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.UserId != userId) return null;
            post.Content = updatePostDto.Content;
            await _context.SaveChangesAsync();
            return await GetPostByIdAsync(postId, userId);
        }

        public async Task<bool> DeletePostAsync(int postId, int userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.UserId != userId) return false;
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<IEnumerable<PostDto>> GetTimelinePostsAsync(int userId)
        {
            // ١. لیستی IDی ئەو کەسانەی کە من فۆڵۆوم کردوون
            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            // ٢. IDی خۆشم زیاد دەکەم بۆ ئەوەی پۆستی خۆشم ببینم
            followingIds.Add(userId);

            // ٣. پۆستی ئەو کەسانە دەهێنین کە لەو لیستەیەدان
            var posts = await _context.Posts
                .Where(p => followingIds.Contains(p.UserId))
                .Include(p => p.User)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => MapToPostDto(p, userId)) // فانکشنە یاریدەدەرەکەی خۆمان بەکاردەهێنین
                .ToListAsync();

            return posts;
        }
        // Services/PostService.cs

        private PostDto MapToPostDto(Post post, int? currentUserId)
        {
            if (post == null)
            {
                return null;
            }

            return new PostDto
            {
                Id = post.Id,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,
                UserId = post.UserId,
                // === گۆڕانکارییەکە لێرەدایە (پشکنینی null زیادکراوە) ===
                Username = post.User?.Username, // ئەگەر User بوونی نەبوو، null دەگەڕێنێتەوە
                UserProfilePicture = post.User?.ProfilePicture, // ئەگەر User بوونی نەبوو، null دەگەڕێنێتەوە
                                                                // =======================================================
                LikesCount = post.Likes?.Count ?? 0, // ئەگەر Likes بوونی نەبوو، 0 دادەنێت
                CommentsCount = post.Comments?.Count ?? 0, // ئەگەر Comments بوونی نەبوو، 0 دادەنێت
                IsLikedByCurrentUser = currentUserId.HasValue && (post.Likes?.Any(l => l.UserId == currentUserId.Value) ?? false)
            };
        }
    }
}
