using DiscordClone.Data.Context;
using DiscordClone.Data.Models;
using DiscordClone.Data.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiscordClone.Data.Repository
{
    public class ChatRoomRepository : IRepository<ChatRoom>
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
                .ToListAsync();
        }

        public async Task<ChatRoom?> GetByIdAsync(Guid id)
        {
            return await _context.ChatRooms.FindAsync(id);
        }

        public async Task UpdateAsync(ChatRoom entity)
        {
            _context.ChatRooms.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
