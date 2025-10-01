using Chrika.Api.Data;
using Chrika.Api.Dtos;
using Chrika.Api.DTOs;
using Chrika.Api.Models; // دڵنیابە ئەمە هەیە
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;

        public SearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SearchResultDto>> SearchAsync(string query)
        {
            var results = new List<SearchResultDto>();
            var lowerCaseQuery = query.ToLower(); // بۆ گەڕانێکی باشتر

            if (string.IsNullOrWhiteSpace(lowerCaseQuery))
            {
                return results;
            }

            // === گەڕان بۆ بەکارهێنەران ===
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => u.Username.ToLower().Contains(lowerCaseQuery) ||
                            (u.FirstName + " " + u.LastName).ToLower().Contains(lowerCaseQuery))
                .Take(5)
                .Select(u => new UserSearchResultDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = $"{u.FirstName} {u.LastName}",
                    ProfilePicture = u.ProfilePicture
                })
                .ToListAsync();

            results.AddRange(users.Select(u => new SearchResultDto { ResultType = "User", Data = u }));

            // === گەڕان بۆ پۆستەکان ===
            var posts = await _context.Posts
                .AsNoTracking()
                .Where(p => p.Content.ToLower().Contains(lowerCaseQuery))
                .Include(p => p.User)
                .Take(5)
                .Select(p => new PostSearchResultDto
                {
                    Id = p.Id,
                    ContentSnippet = p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content,
                    AuthorUsername = p.User.Username
                })
                .ToListAsync();

            results.AddRange(posts.Select(p => new SearchResultDto { ResultType = "Post", Data = p }));

            // === START: بەشی زیادکراو ===
            // === گەڕان بۆ گرووپە گشتییەکان ===
            var groups = await _context.Groups // دڵنیابە ناوی خشتەکە دروستە
                .AsNoTracking()
                .Where(g => g.Type == GroupType.Public && // تەنها گرووپی گشتی
                            (g.Name.ToLower().Contains(lowerCaseQuery) ||
                             g.Username.ToLower().Contains(lowerCaseQuery)))
                .Take(5)
                .Select(g => new GroupSearchResultDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Username = g.Username,
                    ProfilePictureUrl = g.ProfilePictureUrl,
                    MemberCount = _context.GroupMembers.Count(m => m.GroupId == g.Id)
                })
                .ToListAsync();

            results.AddRange(groups.Select(g => new SearchResultDto { ResultType = "Group", Data = g }));
            // === END: بەشی زیادکراو ===

            return results;
        }
    }
}
