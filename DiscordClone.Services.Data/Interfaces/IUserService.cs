using DiscordClone.ViewModels;
using DiscordClone.ViewModels.User;
using Microsoft.AspNetCore.Http;

namespace DiscordClone.Services.Data.Interfaces
{

    public interface IUserService
    {

        /// <param name="username">The username to authenticate</param>
        /// <param name="httpContext">The HTTP context for the current request</param>
        /// <returns>True if login was successful, false otherwise</returns>
        Task<bool> LoginAsync(string username, HttpContext httpContext);
        

        /// <param name="httpContext">The HTTP context for the current request</param>
        /// <returns>True if logout was successful</returns>
        Task<bool> LogoutAsync(HttpContext httpContext);
    }
}
