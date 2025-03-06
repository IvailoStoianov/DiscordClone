using DiscordClone.ViewModels;
using DiscordClone.ViewModels.User;
using Microsoft.AspNetCore.Http;
using DiscordClone.Data.Models;

namespace DiscordClone.Services.Data.Interfaces
{
    /// <summary>
    /// Defines operations for user authentication and management
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Authenticates a user and creates a session
        /// </summary>
        /// <param name="username">The username to authenticate</param>
        /// <param name="httpContext">The HTTP context for the current request</param>
        /// <returns>True if login was successful, false otherwise</returns>
        Task<bool> LoginAsync(string username, HttpContext httpContext);
        
        /// <summary>
        /// Logs out the current user and ends their session
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request</param>
        /// <returns>True if logout was successful</returns>
        Task<bool> LogoutAsync(HttpContext httpContext);
        
        /// <summary>
        /// Gets a user by their username
        /// </summary>
        /// <param name="username">The username to look up</param>
        /// <returns>The user if found, null otherwise</returns>
        Task<User> GetUserByUsernameAsync(string username);
    }
}
