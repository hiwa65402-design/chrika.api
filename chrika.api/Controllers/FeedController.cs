using Chrika.Api.Data;
using Chrika.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FeedController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/feed
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetMyFeed()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var userLocation = (await _context.Users.FindAsync(userId))?.Location;

        // 1. وەرگرتنی پۆستی ئەو کەسانەی فۆڵۆت کردوون
        var followingIds = await _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        var feedPosts = await _context.Posts
            .Where(p => followingIds.Contains(p.UserId) && p.IsActive)
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
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
                IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == userId)
            })
            .ToListAsync();

        // 2. وەرگرتنی ڕیکلامە چالاکەکان
        var activeAds = await _context.AdCampaigns
            .Where(c => c.Status == "Active" &&
                           c.EndDate > DateTime.UtcNow &&
                           (c.Audience.Locations.Count == 0 || c.Audience.Locations.Contains(userLocation)))
            .Include(c => c.PagePost.Page.Owner) // دڵنیابە Owner لۆد کراوە
            .Select(c => new PostDto // گۆڕینی پۆستی پەیج بۆ هەمان فۆرماتی PostDto
            {
                Id = c.PagePost.Id,
                Content = c.PagePost.Content,
                ImageUrl = c.PagePost.ImageUrl,
                CreatedAt = c.PagePost.CreatedAt,
                UserId = c.PagePost.Page.OwnerId,
                Username = c.PagePost.Page.Name,
                UserProfilePicture = c.PagePost.Page.ProfilePicture,
                IsSponsoredPost = true
            })
            .ToListAsync();

        // 3. تێکەڵکردنی ڕیکلامەکان لەگەڵ پۆستەکان
        var combinedFeed = new List<PostDto>();
        combinedFeed.AddRange(feedPosts);

        int adIndex = 0;
        for (int i = 5; i < combinedFeed.Count; i += 6)
        {
            if (adIndex < activeAds.Count)
            {
                combinedFeed.Insert(i, activeAds[adIndex]);
                adIndex++;
            }
        }
        while (adIndex < activeAds.Count)
        {
            combinedFeed.Add(activeAds[adIndex]);
            adIndex++;
        }

        return Ok(combinedFeed);
    }
}
