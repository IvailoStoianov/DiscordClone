using Microsoft.AspNetCore.Mvc;
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
            var result = await _userService.LoginAsync(model.Username, HttpContext);
            return result ? Ok() : Unauthorized();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _userService.LogoutAsync(HttpContext);
            return result ? Ok() : NotFound();
        }
    }
}