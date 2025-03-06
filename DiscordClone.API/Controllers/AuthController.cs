using Microsoft.AspNetCore.Mvc;
using DiscordClone.Services.Data.Interfaces;
using DiscordClone.ViewModels.User;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace DiscordClone.API.Controllers
{
    /// <summary>
    /// Handles user authentication operations
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Authenticates a user and creates a session
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginInputModel model)
        {
            var result = await _userService.LoginAsync(model.Username, HttpContext);
            if (!result)
            {
                return BadRequest(new { message = "Login failed" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(new { username = model.Username, id = userId });
        }

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        [HttpGet("verify")]
        public IActionResult VerifyAuthentication()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var username = User.FindFirstValue(ClaimTypes.Name);
                
                return Ok(new { 
                    isAuthenticated = true,
                    username = username,
                    id = userId,
                    message = "User is authenticated"
                });
            }
            
            return Unauthorized(new { 
                isAuthenticated = false,
                message = "User is not authenticated. Please log in with a valid username (3-50 characters)." 
            });
        }

        /// <summary>
        /// Logs out the current user and ends their session
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync(HttpContext);
            return Ok(new { message = "Logged out successfully" });
        }
    }
}