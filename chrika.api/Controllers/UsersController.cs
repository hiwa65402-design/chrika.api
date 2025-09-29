using Chrika.Api.Dtos;
using Chrika.Api.DTOs;
using Chrika.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace Chrika.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IFollowService _followService;
        private readonly IFileService _fileService; 

        public UsersController(IUserService userService, IFollowService followService, IFileService fileService) // <-- نوێکرایەوە
        {
            _userService = userService;
            _followService = followService;
            _fileService = fileService; 
        }


        // === گۆڕانکاری لێرەدایە ===
        [HttpPost("authenticate")]
        public async Task<ActionResult<LoginResponseDto>> Authenticate(LoginDto loginDto)
        {
            var response = await _userService.AuthenticateUserAsync(loginDto);

            if (response == null)
                return Unauthorized("Invalid username/email or password.");

            return Ok(response);
        }

        #region Other Endpoints
        [HttpGet]
        [Authorize]

        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound($"User with ID {id} not found.");
            return Ok(user);
        }

        [HttpGet("{id}/profile")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile(int id)
        {
            var userProfile = await _userService.GetUserProfileAsync(id);
            if (userProfile == null) return NotFound($"User with ID {id} not found.");
            return Ok(userProfile);
        }

        [HttpGet("username/{username}")]
        public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null) return NotFound($"User with username '{username}' not found.");
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            if (await _userService.UsernameExistsAsync(createUserDto.Username))
                return BadRequest($"Username '{createUserDto.Username}' is already taken.");

            if (await _userService.EmailExistsAsync(createUserDto.Email))
                return BadRequest($"Email '{createUserDto.Email}' is already registered.");

            var user = await _userService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            var user = await _userService.UpdateUserAsync(id, updateUserDto);
            if (user == null) return NotFound($"User with ID {id} not found.");
            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success) return NotFound($"User with ID {id} not found.");
            return NoContent();
        }

        [HttpGet("check-username/{username}")]
        public async Task<ActionResult<object>> CheckUsername(string username)
        {
            var exists = await _userService.UsernameExistsAsync(username);
            return Ok(new { username, available = !exists });
        }

        [HttpGet("check-email/{email}")]
        public async Task<ActionResult<object>> CheckEmail(string email)
        {
            var exists = await _userService.EmailExistsAsync(email);
            return Ok(new { email, available = !exists });
        }
        #endregion

        [HttpPost("{id}/follow")]
        [Authorize]
        public async Task<IActionResult> ToggleFollow(int id)
        {
            // وەرگرتنی IDی ئەو کەسەی کە داواکارییەکەی ناردووە (follower)
            var followerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (followerId == null) return Unauthorized();

            // id: IDی ئەو کەسەیە کە دەمانەوێت فۆڵۆی بکەین (following)
            var success = await _followService.ToggleFollowAsync(int.Parse(followerId), id);

            if (!success)
            {
                // یان بەکارهێنەرەکە بوونی نییە، یان هەوڵی داوە خۆی فۆڵۆ بکات
                return BadRequest("Unable to follow this user.");
            }

            return Ok();
        }
        // === Endpoint ـی نوێ بۆ گۆڕینی پاسۆرد ===
        // POST: api/users/change-password
        [HttpPost("change-password")]
        [Authorize] // تەنها بەکارهێنەری لۆگینبوو دەتوانێت پاسۆردی خۆی بگۆڕێت
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _userService.ChangePasswordAsync(int.Parse(userId), changePasswordDto);

            if (!success)
            {
                return BadRequest("Incorrect current password or user not found.");
            }

            return Ok("Password changed successfully.");
        }
        [HttpPost("profile-picture")]
        [Authorize]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // 1. وێنەکە خەزن بکە و URL ـەکەی وەربگرە
            var imageUrl = await _fileService.SaveProfilePictureAsync(file);

            // 2. URL ـەکە لە داتابەیس بۆ بەکارهێنەرەکە خەزن بکە
            var success = await _userService.UpdateProfilePictureAsync(userId, imageUrl);

            if (!success)
                return NotFound("User not found.");

            return Ok(new { profilePictureUrl = imageUrl });
        }
    
}
}
