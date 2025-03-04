using DiscordClone.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordClone.Data.Repository.Interfaces
{
    public interface IUserRepository
    {
        public Task<User?> GetByUsernameAsync(string username);
    }
}
