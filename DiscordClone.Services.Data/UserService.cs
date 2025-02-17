using DiscordClone.Data;
using DiscordClone.Data.Context;
using DiscordClone.Data.Models;
using DiscordClone.Services.Data.Interfaces;
using DiscordClone.ViewModels;
using DiscordClone.ViewModels.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordClone.Services.Data
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private static readonly HashSet<string> _activeUsers = new();

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserViewModel?> LoginAsync(string username)
        {
            // Try to find existing user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username);

            // If user doesn't exist, create new one
            if (user == null)
            {
                user = new User
                {
                    UserName = username,
                    Id = Guid.NewGuid()
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            _activeUsers.Add(username);

            return new UserViewModel
            {
                Id = user.Id,
                Username = user.UserName!
            };
        }

        public Task<bool> LogoutAsync(string username)
        {
            return Task.FromResult(_activeUsers.Remove(username));
        }

        public Task<List<string>> GetActiveUsersAsync()
        {
            return Task.FromResult(_activeUsers.ToList());
        }
    }
}
