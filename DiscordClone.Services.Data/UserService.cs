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

        public async Task<bool> LoginAsync(string username, HttpContext httpContext)
        {
            try
            {
                // Try to find existing user
                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    // Create new user if not exists
                    user = new User { UserName = username };
                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        return false;
                    }
                }

                // Create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("UserId", user.Id.ToString()) // Additional claim for redundancy
                };
               
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                    AllowRefresh = true
                };

                // Add logging or debugging here
                Console.WriteLine($"User ID: {user.Id}");
                Console.WriteLine($"Claims Count: {claims.Count}");
                Console.WriteLine($"Authentication Scheme: {CookieAuthenticationDefaults.AuthenticationScheme}");

                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

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

        public async Task<bool> LogoutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return true;
        }
    }
}
