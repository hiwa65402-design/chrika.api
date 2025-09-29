using Chrika.Api.Data;
using Chrika.Api.Dtos;
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

        // 1. پشکنینی پۆستی پەیجەکە
        var pagePost = await _context.PagePosts
            .Include(pp => pp.Page) // بۆ پشکنینی خاوەندارێتی پەیج
            .FirstOrDefaultAsync(pp => pp.Id == createDto.PagePostId);

        if (pagePost == null)
        {
            return NotFound("The specified page post does not exist.");
        }

        // 2. پشکنینی خاوەندارێتی پەیج
        if (pagePost.Page.OwnerId != userId)
        {
            return Forbid("You can only create campaigns for posts on your own pages.");
        }

        // 3. پشکنینی مێژووی کەمپەین
        if (createDto.EndDate <= createDto.StartDate || createDto.StartDate < DateTime.UtcNow)
        {
            return BadRequest("End date must be after the start date, and start date cannot be in the past.");
        }

        // 4. دروستکردنی کەمپەین
        var campaign = new AdCampaign
        {
            PagePostId = createDto.PagePostId,
            Budget = createDto.Budget,
            Currency = createDto.Currency,
            StartDate = createDto.StartDate,
            EndDate = createDto.EndDate,
            Status = "Draft", // هەموو کەمپەینێک سەرەتا وەک 'Draft' دروست دەبێت
            Audience = new TargetAudience // گۆڕینی DTO بۆ مۆدێل
            {
                Locations = createDto.Audience.Locations,
                MinAge = createDto.Audience.MinAge,
                MaxAge = createDto.Audience.MaxAge,
                Gender = createDto.Audience.Gender
            }
        };

        _context.AdCampaigns.Add(campaign);
        await _context.SaveChangesAsync();

        // 5. گەڕاندنەوەی DTO
        var campaignDto = new AdCampaignDto
        {
            Id = campaign.Id,
            PagePostId = campaign.PagePostId,
            Status = campaign.Status,
            Budget = campaign.Budget,
            Currency = campaign.Currency,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Audience = createDto.Audience // DTO ـی Audience وەک خۆی دەگەڕێنینەوە
        };

        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, campaignDto);
    }

    // GET: api/adcampaigns/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<AdCampaignDto>> GetCampaign(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var campaign = await _context.AdCampaigns
            .Include(c => c.PagePost.Page) // بۆ پشکنینی خاوەندارێتی
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null)
        {
            return NotFound();
        }

        // تەنها خاوەنی پەیج دەتوانێت کەمپەینەکەی ببینێت
        if (campaign.PagePost.Page.OwnerId != userId)
        {
            return Forbid();
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
}
