using Chrika.Api.Data;
using Chrika.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SponsorshipController : ControllerBase
{
    /*  // <<-- لێرەوە دەست پێبکە

    private readonly ApplicationDbContext _context;

    public SponsorshipController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/sponsorship/posts/{postId}
    [HttpPost("posts/{postId}")]
    public async Task<IActionResult> SponsorPost(int postId, SponsorPostDto sponsorDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var post = await _context.Posts.FindAsync(postId);

        // دڵنیابە پۆستەکە بوونی هەیە و هی خودی بەکارهێنەرەکەیە
        if (post == null || post.UserId != userId)
        {
            return Forbid(); // یان NotFound()
        }

        if (sponsorDto.NumberOfDays <= 0)
        {
            return BadRequest("Number of days must be positive.");
        }

        // لێرەدا دەتوانین لۆجیکی پارەدان زیاد بکەین، بەڵام بۆ ئێستا وا دایدەنێین کە پارەکە دراوە
        // لێرەدا دەتوانین لۆجیکی پارەدان زیاد بکەین، بەڵام بۆ ئێستا وا دایدەنێین کە پارەکە دراوە

        post.IsSponsored = true; 
        post.SponsoredUntil = (post.SponsoredUntil > DateTime.UtcNow ? post.SponsoredUntil : DateTime.UtcNow)
                              .Value.AddDays(sponsorDto.NumberOfDays);

        post.TargetLocation = sponsorDto.TargetLocation;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Post sponsored successfully.", sponsoredUntil = post.SponsoredUntil, targetLocation = post.TargetLocation });
    }

    */ // <<-- لێرە کۆتایی پێ بهێنە
}
