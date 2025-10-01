using Chrika.Api.Data;
using Chrika.Api.DTOs;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class GroupPostService : IGroupPostService
    {
        private readonly ApplicationDbContext _context;

        public GroupPostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GroupPostDto?> CreateGroupPostAsync(int groupId, CreateGroupPostDto createDto, int authorId)
        {
            // 1. پشکنینی دەسەڵات: ئایا بەکارهێنەرەکە ئەندامی گرووپەکەیە؟
            var isMember = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == groupId && m.UserId == authorId);

            if (!isMember)
            {
                return null; // ئەگەر ئەندام نەبێت، ناتوانێت پۆست بکات
            }

            // 2. دروستکردنی پۆستی نوێ
            var newPost = new GroupPost
            {
                GroupId = groupId,
                AuthorId = authorId,
                Content = createDto.Content,
                PostType = createDto.PostType,
                MediaUrl = createDto.MediaUrl
            };

            _context.GroupPosts.Add(newPost);
            await _context.SaveChangesAsync();

            // 3. گەڕاندنەوەی پۆستە نوێیەکە وەک DTO
            var author = await _context.Users.FindAsync(authorId);
            return new GroupPostDto
            {
                Id = newPost.Id,
                Content = newPost.Content,
                PostType = newPost.PostType.ToString(),
                MediaUrl = newPost.MediaUrl,
                CreatedAt = newPost.CreatedAt,
                AuthorId = author.Id,
                AuthorUsername = author.Username,
                AuthorProfilePicture = author.ProfilePicture
            };
        }

        public async Task<IEnumerable<GroupPostDto>?> GetPostsForGroupAsync(int groupId, int currentUserId)
        {
            // 1. پشکنینی دەسەڵات: ئایا بەکارهێنەرەکە دەتوانێت پۆستەکان ببینێت؟
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null) return null;

            if (group.Type == GroupType.Private)
            {
                var isMember = await _context.GroupMembers
                    .AnyAsync(m => m.GroupId == groupId && m.UserId == currentUserId);
                if (!isMember) return null; // ئەگەر گرووپەکە Private بێت و بەکارهێنەر ئەندام نەبێت
            }

            // 2. هێنانی پۆستەکان
            return await _context.GroupPosts
                .Where(p => p.GroupId == groupId)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.Author)
                .Select(p => new GroupPostDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    PostType = p.PostType.ToString(),
                    MediaUrl = p.MediaUrl,
                    CreatedAt = p.CreatedAt,
                    AuthorId = p.AuthorId,
                    AuthorUsername = p.Author.Username,
                    AuthorProfilePicture = p.Author.ProfilePicture
                })
                .ToListAsync();
        }
    }
}
