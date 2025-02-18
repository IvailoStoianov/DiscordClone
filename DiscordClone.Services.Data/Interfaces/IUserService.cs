using DiscordClone.ViewModels;
using DiscordClone.ViewModels.User;
using Microsoft.AspNetCore.Http;

namespace DiscordClone.Services.Data.Interfaces
{
    public interface IUserService
    {
        Task<bool> LoginAsync(string username, HttpContext httpContext);
        Task<bool> LogoutAsync(HttpContext httpContext);
    }
}
