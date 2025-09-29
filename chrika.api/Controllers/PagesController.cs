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
public class PagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/pages
    [HttpPost]
    public async Task<ActionResult<PageDto>> CreatePage([FromBody] CreatePageDto createPageDto)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var page = new Page
        {
            Name = createPageDto.Name,
            Description = createPageDto.Description,
            OwnerId = ownerId
        };

        _context.Pages.Add(page);
        await _context.SaveChangesAsync();

        // بۆ وەرگرتنی ناوی خاوەنی پەیجەکە
        var owner = await _context.Users.FindAsync(ownerId);

        var pageDto = new PageDto
        {
            Id = page.Id,
            Name = page.Name,
            Description = page.Description,
            OwnerUsername = owner.Username,
            CreatedAt = page.CreatedAt
        };

        return CreatedAtAction(nameof(GetPage), new { id = page.Id }, pageDto);
    }

    // GET: api/pages/{id}
    [HttpGet("{id}")]
    [AllowAnonymous] // با هەموو کەس بتوانێت پەیجەکان ببینێت
    public async Task<ActionResult<PageDto>> GetPage(int id)
    {
        var page = await _context.Pages
            .Include(p => p.Owner) // بۆ وەرگرتنی زانیاری خاوەنەکەی
            .FirstOrDefaultAsync(p => p.Id == id);

        if (page == null)
        {
            return NotFound();
        }

        var pageDto = new PageDto
        {
            Id = page.Id,
            Name = page.Name,
            Description = page.Description,
            ProfilePicture = page.ProfilePicture,
            CoverPicture = page.CoverPicture,
            OwnerUsername = page.Owner.Username,
            CreatedAt = page.CreatedAt
        };

        return Ok(pageDto);
    }
}
