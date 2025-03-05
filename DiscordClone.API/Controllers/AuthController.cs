using Microsoft.AspNetCore.Mvc;
using DiscordClone.Services.Data.Interfaces;
using DiscordClone.ViewModels.User;
using System.Security.Claims;

namespace DiscordClone.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

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

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync(HttpContext);
            return Ok(new { message = "Logged out successfully" });
        }
    }
}