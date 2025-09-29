using Chrika.Api.Data;
using Chrika.Api.Dtos;
using Chrika.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/notifications
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .Include(n => n.TriggeredByUser)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = GenerateMessage(n.Type, n.TriggeredByUser.Username),
                TriggeredByUsername = n.TriggeredByUser.Username,
                TriggeredByProfilePicture = n.TriggeredByUser.ProfilePicture,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead,
                EntityId = n.EntityId
            })
            .ToListAsync();

        return Ok(notifications);
    }

    // POST: api/notifications/mark-as-read
    [HttpPost("mark-as-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    private string GenerateMessage(NotificationType type, string username)
    {
        return type switch
        {
            NotificationType.NewFollower => $"{username} started following you.",
            NotificationType.NewLike => $"{username} liked your post.",
            NotificationType.NewComment => $"{username} commented on your post.",
            _ => "You have a new notification."
        };
    }
}
