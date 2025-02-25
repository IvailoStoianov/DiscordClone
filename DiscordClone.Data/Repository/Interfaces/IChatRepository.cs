using DiscordClone.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordClone.Data.Repository.Interfaces
{
    public interface IChatRoomRepository
    {
        Task<IEnumerable<User>> GetChatRoomMembersAsync(Guid chatRoomId);
        Task<bool> IsUserInChatRoomAsync(Guid userId, Guid chatRoomId);
        Task<bool> AddUserToChatRoomAsync(Guid userId, Guid chatRoomId);
        Task<bool> RemoveUserFromChatRoomAsync(Guid userId, Guid chatRoomId);
        Task<IEnumerable<ChatRoom>> GetChatRoomsForUserAsync(Guid userId);
    }
}
