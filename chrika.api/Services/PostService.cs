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
                .OrderByDescending(item => item.CreatedAt)
                .ToList();

            // دانانی ڕیکلامەکان
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

        public async Task<IEnumerable<PostDto>> GetAllPostsAsync(int? currentUserId = null)
        {
            return await _context.Posts.AsNoTracking().OrderByDescending(p => p.CreatedAt)
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
                }).ToListAsync();
        }

        public async Task<PostDto?> GetPostByIdAsync(int id, int? currentUserId = null)
        {
            return await _context.Posts.AsNoTracking().Where(p => p.Id == id)
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
                }).FirstOrDefaultAsync();
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
    }
}
