using Chrika.Api.DTOs;
using Chrika.Api.Helpers;
using Chrika.Api.Models;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    // Endpoint بۆ تۆمارکردنی کلیک
    // POST: api/analytics/record-click
    [HttpPost("record-click")]
    public async Task<IActionResult> RecordClick([FromBody] AdClickDto clickDto)
    {
        if (clickDto == null || clickDto.AdCampaignId <= 0)
            return BadRequest("Invalid campaign ID.");

        var userId = User.GetUserId();
        await _analyticsService.RecordInteractionAsync(clickDto.AdCampaignId, InteractionType.Click, userId);
        return Ok();
    }

    // === زیادکرا: Endpoint بۆ تۆمارکردنی بینین ===
    // POST: api/analytics/record-impression
    [HttpPost("record-impression")]
    public async Task<IActionResult> RecordImpression([FromBody] AdImpressionDto impressionDto)
    {
        if (impressionDto == null || impressionDto.AdCampaignId <= 0)
            return BadRequest("Invalid campaign ID.");

        var userId = User.GetUserId();
        await _analyticsService.RecordImpressionAsync(impressionDto.AdCampaignId, userId);
        return Ok();
    }

    // === زیادکرا: Endpoint بۆ پیشاندانی ئامارەکان ===
    // GET: api/analytics/campaign/{campaignId}
    [HttpGet("campaign/{campaignId}")]
    public async Task<IActionResult> GetCampaignAnalytics(int campaignId)
    {
        var analytics = await _analyticsService.GetCampaignAnalyticsAsync(campaignId);
        if (analytics == null)
        {
            return NotFound($"Campaign with ID {campaignId} not found.");
        }
        return Ok(analytics);
    }
}
