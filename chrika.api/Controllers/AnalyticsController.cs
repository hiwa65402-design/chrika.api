using Chrika.Api.DTOs;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Chrika.Api.Models; // <-- ئەم هێڵە گرنگە زیاد بکە

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

    // POST: api/analytics/record-click
    [HttpPost("record-click")]
    public async Task<IActionResult> RecordClick([FromBody] AdClickDto clickDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (clickDto == null || clickDto.AdCampaignId <= 0)
        {
            return BadRequest("Invalid campaign ID.");
        }

        // ئێستا ئەم هێڵە بە دروستی کار دەکات
        await _analyticsService.RecordInteractionAsync(clickDto.AdCampaignId, InteractionType.Click, userId);

        return Ok();
    }
}
