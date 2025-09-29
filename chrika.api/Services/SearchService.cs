using Chrika.Api.Data;
using Chrika.Api.Dtos;
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

            if (string.IsNullOrWhiteSpace(query))
            {
                return results;
            }

            // گەڕان بۆ بەکارهێنەران (بەپێی ناوی بەکارهێنەر، ناوی یەکەم، ناوی دووەم)
            var users = await _context.Users
                .Where(u => u.Username.Contains(query) ||
                            u.FirstName.Contains(query) ||
                            u.LastName.Contains(query))
                .Take(5) // بۆ نموونە، تەنها 5 ئەنجامی یەکەم
                .Select(u => new UserSearchResultDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = $"{u.FirstName} {u.LastName}",
                    ProfilePicture = u.ProfilePicture
                })
                .ToListAsync();

            foreach (var user in users)
            {
                results.Add(new SearchResultDto { ResultType = "User", Data = user });
            }

            // گەڕان بۆ پۆستەکان (بەپێی ناوەڕۆک)
            var posts = await _context.Posts
                .Where(p => p.Content.Contains(query))
                .Include(p => p.User)
                .Take(5) // تەنها 5 ئەنجامی یەکەم
                .Select(p => new PostSearchResultDto
                {
                    Id = p.Id,
                    // پارچەیەکی 100 پیتی لە ناوەڕۆکەکە دەبڕین
                    ContentSnippet = p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content,
                    AuthorUsername = p.User.Username
                })
                .ToListAsync();

            foreach (var post in posts)
            {
                results.Add(new SearchResultDto { ResultType = "Post", Data = post });
            }

            return results;
        }
    }
}
