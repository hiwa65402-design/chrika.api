using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Chrika.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // === گرنگترین بەش: تەنها ئەدمینەکان بۆیان هەیە ئەم کۆنترۆڵەرە بەکاربهێنن ===
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/admin/users
        // وەرگرتنی لیستی هەموو بەکارهێنەران (تەنها بۆ ئەدمین)
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // DELETE: api/admin/users/5
        // سڕینەوەی بەکارهێنەرێک لەلایەن ئەدمینەوە
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound("User not found.");
            }
            return NoContent();
        }
    }
}
