using DiscordClone.Data.Context;
using DiscordClone.Data.Models;
using DiscordClone.Data.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiscordClone.Data.Repository
{
    public class ChatRoomRepository : IRepository<ChatRoom>, IChatRoomRepository
    {
        private readonly ApplicationDbContext _context;
        public ChatRoomRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ChatRoom entity)
        {
            _context.ChatRooms.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var chatRoom = await _context
                .ChatRooms
                .FindAsync(id);

            if (chatRoom == null)
            {
                throw new Exception("Chat room not found");
            }
            _context.ChatRooms.Remove(chatRoom);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ChatRoom>> GetAllAsync()
        {
            return await _context
                .ChatRooms
                .Include(c => c.Owner)
                .ToListAsync();
        }

        public async Task<ChatRoom?> GetByIdAsync(Guid id)
        {
            return await _context.ChatRooms
                .Include(c => c.Owner)
                .Include(c => c.Users)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateAsync(ChatRoom entity)
        {
            _context.ChatRooms.Update(entity);
            await _context.SaveChangesAsync();
        }

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

        public async Task<IEnumerable<User>> GetChatRoomMembersAsync(Guid chatRoomId)
        {
            var chatRoom = await _context.ChatRooms
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatRoomId);

            if (chatRoom == null)
            {
                return new List<User>();
            }

            return chatRoom.Users.ToList();
        }

        public async Task<bool> IsUserInChatRoomAsync(Guid userId, Guid chatRoomId)
        {
            return await _context.ChatRoomUsers
                .AnyAsync(cru => cru.UserId == userId && cru.ChatRoomId == chatRoomId);
        }

        public async Task<bool> AddUserToChatRoomAsync(Guid userId, Guid chatRoomId)
        {
            // Check if the relationship already exists
            var exists = await _context.ChatRoomUsers
                .AnyAsync(cru => cru.UserId == userId && cru.ChatRoomId == chatRoomId);

            if (exists)
            {
                return true; // User is already in the chat room
            }

            // Check if both user and chat room exist
            var user = await _context.Users.FindAsync(userId);
            var chatRoom = await _context.ChatRooms.FindAsync(chatRoomId);

            if (user == null || chatRoom == null)
            {
                return false; // User or chat room doesn't exist
            }

            // Add the relationship
            await _context.ChatRoomUsers.AddAsync(new ChatRoomUser
            {
                UserId = userId,
                ChatRoomId = chatRoomId
            });

            await _context.SaveChangesAsync();
            return true;
        }

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

        public async Task<IEnumerable<ChatRoom>> GetChatRoomsForUserAsync(Guid userId)
        {
            return await _context.ChatRooms
                .Where(c => c.Users.Any(u => u.Id == userId) || c.OwnerId == userId)
                .Include(c => c.Owner)
                .ToListAsync();
        }
    }
}
