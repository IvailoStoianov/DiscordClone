using DiscordClone.Data.Models;
using DiscordClone.Services.Data.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DiscordClone.Services.Data
{

    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }


        /// <param name="username">The username to authenticate</param>
        /// <param name="httpContext">The HTTP context for the current request</param>
        /// <returns>True if login was successful, false otherwise</returns>
        public async Task<bool> LoginAsync(string username, HttpContext httpContext)
        {
            try
            {
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
                        return false;
                    }
                }

                // Create claims for the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                // Create identity and sign in
                var identity = new ClaimsIdentity(claims, "cookie");
                var principal = new ClaimsPrincipal(identity);

                await httpContext.SignInAsync("Cookies", principal, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

                // Verify the user is authenticated after sign in
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    Console.WriteLine("User successfully authenticated");
                }

                Console.WriteLine("\n=== Login Details ===");
                Console.WriteLine($"User ID: {user.Id}");
                Console.WriteLine($"Username: {user.UserName}");
                Console.WriteLine("=== Claims Set ===");
                foreach (var claim in claims)
                {
                    Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Login error: {ex.Message}");
                return false;
            }
        }

        /// <param name="httpContext">The HTTP context for the current request</param>
        /// <returns>True if logout was successful</returns>
        public async Task<bool> LogoutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync("Cookies");
            return true;
        }
    }
}
