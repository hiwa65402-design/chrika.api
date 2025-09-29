using Chrika.Api.Data;
using Chrika.Api.Dtos;
using Chrika.Api.DTOs;
using Chrika.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AdCampaignsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdCampaignsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/adcampaigns
    [HttpPost]
    public async Task<ActionResult<AdCampaignDto>> CreateCampaign([FromBody] CreateAdCampaignDto createDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var pagePost = await _context.PagePosts
            .Include(pp => pp.Page)
            .FirstOrDefaultAsync(pp => pp.Id == createDto.PagePostId);

        if (pagePost == null)
        {
            return NotFound("The specified page post does not exist.");
        }

        // === گۆڕانکاری یەکەم ===
        if (pagePost.Page.OwnerId != userId)
        {
            // return Forbid("You can only create campaigns for posts on your own pages.");
            return StatusCode(403, "You can only create campaigns for posts on your own pages.");
        }

        if (createDto.EndDate <= createDto.StartDate || createDto.StartDate < DateTime.UtcNow)
        {
            return BadRequest("End date must be after the start date, and start date cannot be in the past.");
        }

        var campaign = new AdCampaign
        {
            PagePostId = createDto.PagePostId,
            Budget = createDto.Budget,
            Currency = createDto.Currency,
            StartDate = createDto.StartDate,
            EndDate = createDto.EndDate,
            Status = "Draft",
            Audience = new TargetAudience
            {
                Locations = createDto.Audience.Locations,
                MinAge = createDto.Audience.MinAge,
                MaxAge = createDto.Audience.MaxAge,
                Gender = createDto.Audience.Gender
            }
        };

        _context.AdCampaigns.Add(campaign);
        await _context.SaveChangesAsync();

        var campaignDto = new AdCampaignDto
        {
            Id = campaign.Id,
            PagePostId = campaign.PagePostId,
            Status = campaign.Status,
            Budget = campaign.Budget,
            Currency = campaign.Currency,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Audience = createDto.Audience
        };

        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, campaignDto);
    }

    // GET: api/adcampaigns/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<AdCampaignDto>> GetCampaign(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var campaign = await _context.AdCampaigns
            .Include(c => c.PagePost.Page)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null)
        {
            return NotFound();
        }

        // === گۆڕانکاری دووەم ===
        if (campaign.PagePost.Page.OwnerId != userId)
        {
            // return Forbid();
            return StatusCode(403, "You do not have permission to view this campaign.");
        }

        var campaignDto = new AdCampaignDto
        {
            Id = campaign.Id,
            PagePostId = campaign.PagePostId,
            Status = campaign.Status,
            Budget = campaign.Budget,
            Currency = campaign.Currency,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Audience = new TargetAudienceDto
            {
                Locations = campaign.Audience.Locations,
                MinAge = campaign.Audience.MinAge,
                MaxAge = campaign.Audience.MaxAge,
                Gender = campaign.Audience.Gender
            }
        };

        return Ok(campaignDto);
    }

    // POST: api/adcampaigns/{id}/launch
    [HttpPost("{id}/launch")]
    public async Task<IActionResult> LaunchCampaign(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var campaign = await _context.AdCampaigns
            .Include(c => c.PagePost.Page)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null)
        {
            return NotFound("Campaign not found.");
        }

        // === گۆڕانکاری سێیەم ===
        if (campaign.PagePost.Page.OwnerId != userId)
        {
            // return Forbid("You can only launch campaigns for your own pages.");
            return StatusCode(403, "You can only launch campaigns for your own pages.");
        }

        if (campaign.Status != "Draft")
        {
            return BadRequest($"Campaign cannot be launched. Current status: {campaign.Status}.");
        }

        if (campaign.EndDate < DateTime.UtcNow)
        {
            return BadRequest("This campaign has already expired and cannot be launched.");
        }

        campaign.Status = "Active";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Campaign launched successfully!", campaignId = campaign.Id, newStatus = campaign.Status });
    }
}
