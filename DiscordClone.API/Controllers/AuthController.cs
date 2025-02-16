using Microsoft.AspNetCore.Mvc;
using DiscordClone.ViewModels;
using DiscordClone.Services.Data.Interfaces;
using DiscordClone.ViewModels.User;

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
            if (string.IsNullOrWhiteSpace(model.Username))
            {
                return BadRequest("Username cannot be empty");
            }

            var user = await _userService.LoginAsync(model.Username);
            if (user == null)
            {
                return Conflict("Username is already taken");
            }

            return Ok(user);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string username)
        {
            var result = await _userService.LogoutAsync(username);
            return result ? Ok() : NotFound();
        }

        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsername([FromQuery] string username)
        {
            var isAvailable = await _userService.IsUsernameAvailableAsync(username);
            return Ok(new { isAvailable });
        }
    }
}