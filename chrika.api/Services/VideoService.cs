// Services/VideoService.cs

using Chrika.Api.Data;
using Chrika.Api.DTOs;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class VideoService : IVideoService
    {
        private readonly ApplicationDbContext _context;

        public VideoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VideoFeedItemDto>> GetVideoFeedAsync(int? userId, int pageNumber, int pageSize)
        {
            // 1. هێنانی ڤیدیۆی پۆستە ئاساییەکان
            var userVideoPosts = _context.Posts
                .Where(p => p.ImageUrl != null && (p.ImageUrl.EndsWith(".mp4") || p.ImageUrl.EndsWith(".mov")))
                .Select(p => new VideoFeedItemDto
                {
                    Id = p.Id,
                    ItemType = "Post",
                    Content = p.Content,
                    MediaUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    AuthorId = p.UserId,
                    AuthorUsername = p.User.Username,
                    AuthorProfilePicture = p.User.ProfilePicture,
                    GroupId = null,
                    GroupName = null,
                    LikesCount = _context.Likes.Count(l => l.PostId == p.Id),
                    CommentsCount = _context.Comments.Count(c => c.PostId == p.Id),
                    IsLikedByCurrentUser = userId.HasValue && _context.Likes.Any(l => l.PostId == p.Id && l.UserId == userId.Value)
                });

            // 2. هێنانی ڤیدیۆی پۆستە گرووپەکان
            var groupVideoPosts = _context.GroupPosts
                .Where(gp => gp.PostType == GroupPostType.Video)
                .Select(gp => new VideoFeedItemDto
                {
                    Id = gp.Id,
                    ItemType = "GroupPost",
                    Content = gp.Content,
                    MediaUrl = gp.MediaUrl,
                    CreatedAt = gp.CreatedAt,
                    AuthorId = gp.AuthorId,
                    AuthorUsername = gp.Author.Username,
                    AuthorProfilePicture = gp.Author.ProfilePicture,
                    GroupId = gp.GroupId,
                    GroupName = gp.Group.Name,
                    LikesCount = _context.Likes.Count(l => l.GroupPostId == gp.Id),
                    CommentsCount = _context.Comments.Count(c => c.GroupPostId == gp.Id),
                    IsLikedByCurrentUser = userId.HasValue && _context.Likes.Any(l => l.GroupPostId == gp.Id && l.UserId == userId.Value)
                });

            // 3. تێکەڵکردنی هەردوو جۆرەکە
            var allVideos = userVideoPosts.Concat(groupVideoPosts);

            // === گۆڕانکارییەکە لێرەدایە ===
            // 4. ڕیزکردنی هەڕەمەکی و لاپەڕەبەندکردن
            // یەکەمجار هەموو ڤیدیۆکان دەهێنینە ناو memory
            var videoList = await allVideos.ToListAsync();

            // پاشان بە شێوەیەکی هەڕەمەکی ڕیزیان دەکەین
            var random = new System.Random();
            var shuffledVideos = videoList.OrderBy(v => random.Next());

            // لە کۆتاییدا، لاپەڕەبەندکردن جێبەجێ دەکەین
            return shuffledVideos
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}
