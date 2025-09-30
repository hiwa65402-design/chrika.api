using Chrika.Api.Data;
using Chrika.Api.Models;
using Chrika.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class ShareService : IShareService
{
    private readonly ApplicationDbContext _context;
    private readonly IAnalyticsService _analyticsService; // بۆ تۆمارکردنی ئامار

    public ShareService(ApplicationDbContext context, IAnalyticsService analyticsService)
    {
        _context = context;
        _analyticsService = analyticsService;
    }

    public async Task RecordShareAsync(int postId, int userId)
    {
        // دڵنیابە پۆست و بەکارهێنەر بوونیان هەیە
        var postExists = await _context.Posts.AnyAsync(p => p.Id == postId) || await _context.PagePosts.AnyAsync(pp => pp.Id == postId);
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!postExists || !userExists) return;

        // تۆمارکردنی شێرکردن
        var share = new Share { PostId = postId, UserId = userId };
        _context.Shares.Add(share);
        await _context.SaveChangesAsync();

        // === ئەگەر پۆستەکە سپۆنسەر کرابوو، ئامارەکەشی تۆمار بکە ===
        await CheckAndRecordInteraction(postId, InteractionType.Share, userId);
    }

    private async Task CheckAndRecordInteraction(int postId, InteractionType type, int userId)
    {
        var pagePost = await _context.PagePosts.FirstOrDefaultAsync(pp => pp.Id == postId);
        if (pagePost != null)
        {
            var activeCampaign = await _context.AdCampaigns
                .FirstOrDefaultAsync(c => c.PagePostId == postId && c.Status == CampaignStatus.Active && c.EndDate > DateTime.UtcNow);

            if (activeCampaign != null)
            {
                await _analyticsService.RecordInteractionAsync(activeCampaign.Id, type, userId);
            }
        }
    }
}
