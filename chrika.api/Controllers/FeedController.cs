using Chrika.Api.Data;
using Chrika.Api.DTOs;
using Chrika.Api.Models;
using Chrika.Api.Services; // <-- گرنگ: ئەمە زیاد بکە
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAnalyticsService _analyticsService; // <-- زیادکرا

    // === گۆڕانکاری یەکەم: زیادکردنی IAnalyticsService بۆ constructor ===
    public FeedController(ApplicationDbContext context, IAnalyticsService analyticsService)
    {
        _context = context;
        _analyticsService = analyticsService; // <-- زیادکرا
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetMyFeed()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var user = await _context.Users.FindAsync(userId);
        var userLocation = user?.Location;

        // 1. وەرگرتنی پۆستی ئەو کەسانەی فۆڵۆت کردوون (وەک خۆی)
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

        // 2. وەرگرتنی هەموو ڕیکلامە چالاکەکان
        var allActiveCampaigns = await _context.AdCampaigns
            .Where(c => c.Status == CampaignStatus.Active && c.EndDate > DateTime.UtcNow)
            .Include(c => c.PagePost.Page) // <-- تەنها Page لۆد بکە
            .ToListAsync();

        // 3. فلتەرکردنی ڕیکلامەکان و دروستکردنی DTO
        var relevantCampaigns = allActiveCampaigns
            .Where(c => c.Audience.Locations.Count == 0 || (userLocation != null && c.Audience.Locations.Contains(userLocation)))
            .ToList();

        var adDtos = relevantCampaigns.Select(c => new PostDto
        {
            Id = c.PagePost.Id,
            Content = c.PagePost.Content,
            ImageUrl = c.PagePost.ImageUrl,
            CreatedAt = c.PagePost.CreatedAt,
            UserId = c.PagePost.Page.OwnerId,
            Username = c.PagePost.Page.Name,
            UserProfilePicture = c.PagePost.Page.ProfilePicture,
            IsSponsoredPost = true,
            // AdCampaignId پێویستە بۆ تۆمارکردنی کرتە
            AdCampaignId = c.Id
        })
        .ToList();

        // 4. تێکەڵکردنی ڕیکلامەکان لەگەڵ پۆستەکان (وەک خۆی)
        var combinedFeed = new List<PostDto>();
        combinedFeed.AddRange(feedPosts);

        int adIndex = 0;
        for (int i = 5; i < combinedFeed.Count; i += 6)
        {
            if (adIndex < adDtos.Count)
            {
                combinedFeed.Insert(i, adDtos[adIndex]);
                adIndex++;
            }
        }
        while (adIndex < adDtos.Count)
        {
            combinedFeed.Add(adDtos[adIndex]);
            adIndex++;
        }

        // === گۆڕانکاری دووەم: تۆمارکردنی بینینەکان ===
        foreach (var campaign in relevantCampaigns)
        {
            await _analyticsService.RecordInteractionAsync(campaign.Id, InteractionType.Impression, userId);
        }
        // ==========================================

        return Ok(combinedFeed);
    }
}
