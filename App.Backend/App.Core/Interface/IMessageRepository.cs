using App.Core.Entities;

namespace App.Core.Interface
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<List<Message>> GetMessagesWithUsersAsync(string userId);
    }
}