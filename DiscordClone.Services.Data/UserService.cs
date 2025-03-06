using DiscordClone.Data.Models;
using DiscordClone.Data.Repository;
using DiscordClone.Services.Data.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace DiscordClone.Services.Data
{
    /// <summary>
    /// Provides services for user authentication and management
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserService> _logger;
        private readonly UserRepository _userRepository;

        public UserService(UserManager<User> userManager, ILogger<UserService> logger, UserRepository userRepository)
        {
            _userManager = userManager;
            _logger = logger;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Authenticates a user and creates a session
        /// </summary>
        /// <param name="username">The username to authenticate</param>
        /// <param name="httpContext">The HTTP context for the current request</param>
        /// <returns>True if login was successful, false otherwise</returns>
        public async Task<bool> LoginAsync(string username, HttpContext httpContext)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("Login attempt with empty username");
                    return false;
                }

                if (username.Length < 3 || username.Length > 50)
                {
                    _logger.LogWarning($"Login attempt with invalid username length: {username.Length}");
                    return false;
                }

                // Find the user by username
                var user = await _userManager.FindByNameAsync(username);
                
                // If user doesn't exist, create a new one
                if (user == null)
                {
                    user = new User
                    {
                        UserName = username,
                        Email = $"{username}@example.com", // Default email
                        EmailConfirmed = true // Auto-confirm for demo purposes
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        _logger.LogError($"Failed to create user: {errors}");
                        return false;
                    }
                }

                // Create claims for the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email ?? $"{username}@example.com")
                };

                // Create identity and sign in
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

                _logger.LogInformation($"User {username} logged in successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for username: {username}");
                return false;
            }
        }

        /// <summary>
        /// Logs out the current user and ends their session
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request</param>
        /// <returns>True if logout was successful</returns>
        public async Task<bool> LogoutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return true;
        }
        
        /// <summary>
        /// Gets a user by their username
        /// </summary>
        /// <param name="username">The username to look up</param>
        /// <returns>The user if found, null otherwise</returns>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _userRepository.GetByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user by username: {username}");
                return null;
            }
        }
    }
}
