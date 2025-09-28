using Microsoft.AspNetCore.Mvc;
using Chrika.Api.DTOs;
using Chrika.Api.Services;

namespace Chrika.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of users</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            
            if (user == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(user);
        }

        /// <summary>
        /// Get user profile by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User profile with additional statistics</returns>
        [HttpGet("{id}/profile")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile(int id)
        {
            var userProfile = await _userService.GetUserProfileAsync(id);
            
            if (userProfile == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(userProfile);
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User details</returns>
        [HttpGet("username/{username}")]
        public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            
            if (user == null)
                return NotFound($"User with username '{username}' not found.");

            return Ok(user);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="createUserDto">User creation data</param>
        /// <returns>Created user</returns>
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            // Check if username already exists
            if (await _userService.UsernameExistsAsync(createUserDto.Username))
                return BadRequest($"Username '{createUserDto.Username}' is already taken.");

            // Check if email already exists
            if (await _userService.EmailExistsAsync(createUserDto.Email))
                return BadRequest($"Email '{createUserDto.Email}' is already registered.");

            var user = await _userService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="updateUserDto">User update data</param>
        /// <returns>Updated user</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            var user = await _userService.UpdateUserAsync(id, updateUserDto);
            
            if (user == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(user);
        }

        /// <summary>
        /// Delete user (soft delete)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _userService.DeleteUserAsync(id);
            
            if (!success)
                return NotFound($"User with ID {id} not found.");

            return NoContent();
        }

        /// <summary>
        /// Authenticate user (login)
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>User information if authentication successful</returns>
        [HttpPost("authenticate")]
        public async Task<ActionResult<UserDto>> Authenticate(LoginDto loginDto)
        {
            var user = await _userService.AuthenticateUserAsync(loginDto);
            
            if (user == null)
                return Unauthorized("Invalid username/email or password.");

            return Ok(user);
        }

        /// <summary>
        /// Check if username is available
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>Availability status</returns>
        [HttpGet("check-username/{username}")]
        public async Task<ActionResult<object>> CheckUsername(string username)
        {
            var exists = await _userService.UsernameExistsAsync(username);
            return Ok(new { username, available = !exists });
        }

        /// <summary>
        /// Check if email is available
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>Availability status</returns>
        [HttpGet("check-email/{email}")]
        public async Task<ActionResult<object>> CheckEmail(string email)
        {
            var exists = await _userService.EmailExistsAsync(email);
            return Ok(new { email, available = !exists });
        }
    }
}

