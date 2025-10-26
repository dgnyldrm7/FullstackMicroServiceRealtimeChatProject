using App.Core.Entities;
using App.Core.Interface;
using App.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace App.Persistance.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _context;

        public MessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Message>> GetMessagesWithUsersAsync(string userId)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .ToListAsync();
        }

        public async ValueTask<Message> GetByIdAsync(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<IEnumerable<Message>> GetAllAsync()
        {
            return await _context.Messages.ToListAsync();
        }

        public async Task AddAsync(Message entity)
        {
            await _context.Messages.AddAsync(entity);
        }

        public void Update(Message entity)
        {
            _context.Messages.Update(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                _context.Messages.Remove(message);
            }
        }

        public IQueryable<Message> Query()
        {
            return _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver);
        }
    }
}
