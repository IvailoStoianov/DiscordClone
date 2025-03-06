using DiscordClone.Data.Context;
using DiscordClone.Data.Models;
using DiscordClone.Data.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordClone.Data.Repository
{
    /// <summary>
    /// Repository for managing chat room data operations
    /// </summary>
    public class ChatRoomRepository : IRepository<ChatRoom>, IChatRoomRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRoomRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new chat room to the database
        /// </summary>
        public async Task AddAsync(ChatRoom entity)
        {
            await _context.ChatRooms.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Permanently deletes a chat room from the database
        /// </summary>
        public async Task DeleteAsync(Guid id)
        {
            var chatRoom = await _context.ChatRooms.FindAsync(id);
            if (chatRoom != null)
            {
                _context.ChatRooms.Remove(chatRoom);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Gets all chat rooms from the database
        /// </summary>
        public async Task<IEnumerable<ChatRoom>> GetAllAsync()
        {
            return await _context.ChatRooms
                .Include(c => c.Messages)
                .ThenInclude(m => m.User)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a chat room by ID with related messages and users
        /// </summary>
        public async Task<ChatRoom?> GetByIdAsync(Guid id)
        {
            return await _context.ChatRooms
                .Include(c => c.Messages.Where(m => !m.IsDeleted))
                .ThenInclude(m => m.User)
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        /// <summary>
        /// Updates an existing chat room in the database
        /// </summary>
        public async Task UpdateAsync(ChatRoom entity)
        {
            _context.ChatRooms.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Marks a chat room as deleted without removing it from the database
        /// </summary>
        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var chatRoom = await _context.ChatRooms.FindAsync(id);
            if (chatRoom == null)
            {
                return false;
            }

            chatRoom.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Gets all members of a chat room
        /// </summary>
        public async Task<IEnumerable<User>> GetChatRoomMembersAsync(Guid chatRoomId)
        {
            var chatRoom = await _context.ChatRooms
                .Include(c => c.Users)
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(c => c.Id == chatRoomId);

            if (chatRoom == null)
            {
                return new List<User>();
            }

            var members = new List<User>(chatRoom.Users);
            if (!members.Any(u => u.Id == chatRoom.OwnerId))
            {
                members.Add(chatRoom.Owner);
            }

            return members;
        }

        /// <summary>
        /// Checks if a user is a member of a chat room
        /// </summary>
        public async Task<bool> IsUserInChatRoomAsync(Guid userId, Guid chatRoomId)
        {
            var chatRoom = await GetByIdAsync(chatRoomId);
            return chatRoom != null && (chatRoom.OwnerId == userId || chatRoom.Users.Any(u => u.Id == userId));
        }

        /// <summary>
        /// Adds a user to a chat room
        /// </summary>
        public async Task<bool> AddUserToChatRoomAsync(Guid userId, Guid chatRoomId)
        {
            var chatRoom = await _context.ChatRooms
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatRoomId);

            if (chatRoom == null)
            {
                return false;
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (chatRoom.Users.Any(u => u.Id == userId))
            {
                return true; // User is already in the chat room
            }

            chatRoom.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Removes a user from a chat room
        /// </summary>
        public async Task<bool> RemoveUserFromChatRoomAsync(Guid userId, Guid chatRoomId)
        {
            var chatRoomUser = await _context.ChatRoomUsers
                .FirstOrDefaultAsync(cru => cru.UserId == userId && cru.ChatRoomId == chatRoomId);

            if (chatRoomUser == null)
            {
                return false; // User is not in the chat room
            }

            _context.ChatRoomUsers.Remove(chatRoomUser);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Gets all chat rooms that a user is a member of
        /// </summary>
        public async Task<IEnumerable<ChatRoom>> GetChatRoomsForUserAsync(Guid userId)
        {
            return await _context.ChatRooms
                .Where(c => (c.Users.Any(u => u.Id == userId) || c.OwnerId == userId) && c.IsDeleted == false)
                .Include(c => c.Owner)
                .ToListAsync();
        }
    }
}
