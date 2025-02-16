using DiscordClone.ViewModels;
using DiscordClone.ViewModels.User;

namespace DiscordClone.Services.Data.Interfaces
{
    public interface IUserService
    {
        Task<UserViewModel?> LoginAsync(string username);
        Task<bool> LogoutAsync(string username);
        Task<bool> IsUsernameAvailableAsync(string username);
    }
}
