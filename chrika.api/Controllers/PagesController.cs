using Chrika.Api.Data;
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

    // POST: api/pages/{pageId}/posts
    [HttpPost("{pageId}/posts")]
    public async Task<ActionResult<PagePostDto>> CreatePagePost(int pageId, [FromBody] CreatePagePostDto createPostDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // 1. پشکنین بکە بزانە پەیجەکە بوونی هەیە
        var page = await _context.Pages.FindAsync(pageId);
        if (page == null)
        {
            return NotFound("Page not found.");
        }

        // 2. پشکنین بکە بزانە ئایا بەکارهێنەری ئێستا خاوەنی پەیجەکەیە
        if (page.OwnerId != userId)
        {
            return Forbid("You are not the owner of this page.");
        }

        // 3. پۆستە نوێیەکە دروست بکە
        var pagePost = new PagePost
        {
            Content = createPostDto.Content,
            ImageUrl = createPostDto.ImageUrl,
            PageId = pageId
        };

        _context.PagePosts.Add(pagePost);
        await _context.SaveChangesAsync();

        // 4. ئەنجامەکە بگەڕێنەرەوە
        var pagePostDto = new PagePostDto
        {
            Id = pagePost.Id,
            Content = pagePost.Content,
            ImageUrl = pagePost.ImageUrl,
            CreatedAt = pagePost.CreatedAt,
            PageId = pagePost.PageId,
            IsSponsored = pagePost.IsSponsored, // بە دڵنیاییەوە false دەبێت
            SponsoredUntil = pagePost.SponsoredUntil,
            TargetLocation = pagePost.TargetLocation
        };

        // دەتوانین Endpointێکی GetPagePost دروست بکەین، بەڵام بۆ ئێستا با بەم شێوەیە بێت
        return Ok(pagePostDto);
    }

    // PUT: api/pages/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePage(int id, [FromBody] UpdatePageDto updateDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var page = await _context.Pages.FindAsync(id);

        if (page == null)
        {
            return NotFound();
        }

        // پشکنینی خاوەندارێتی
        if (page.OwnerId != userId)
        {
            return Forbid();
        }

        // نوێکردنەوەی ئەو زانیاریانەی کە نێردراون
        page.Name = updateDto.Name ?? page.Name;
        page.Description = updateDto.Description ?? page.Description;
        page.Bio = updateDto.Bio ?? page.Bio;
        page.PhoneNumber = updateDto.PhoneNumber ?? page.PhoneNumber;
        page.WhatsAppNumber = updateDto.WhatsAppNumber ?? page.WhatsAppNumber;
        page.WebsiteUrl = updateDto.WebsiteUrl ?? page.WebsiteUrl;
        page.Location = updateDto.Location ?? page.Location;
        page.Category = updateDto.Category ?? page.Category;

        await _context.SaveChangesAsync();

        return NoContent(); // 204 No Content وەڵامێکی ستانداردە بۆ نوێکردنەوەی سەرکەوتوو
    }

}
