using DiscordClone.Data.Models;
using DiscordClone.Data.Repository.Interfaces;
using DiscordClone.Data.Context;
using Microsoft.EntityFrameworkCore;
namespace DiscordClone.Data.Repository
{
    public class MessageRepository : IRepository<Message>
    {
        private readonly ApplicationDbContext _context;

        public MessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Message entity)
        {
            await _context.Messages.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                throw new Exception("Message not found");
            }
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Message>> GetAllAsync()
        {
            return await _context
                .Messages
                .ToListAsync();
        }

        public async Task<Message?> GetByIdAsync(Guid id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                throw new Exception("Message not found");
            }
            return await _context.Messages.FindAsync(id);
        }

        public async Task UpdateAsync(Message entity)
        {
            _context.Messages.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
