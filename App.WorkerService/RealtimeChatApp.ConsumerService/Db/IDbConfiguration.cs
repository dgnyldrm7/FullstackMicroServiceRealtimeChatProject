using RealtimeChatApp.ConsumerService.Models;

namespace RealtimeChatApp.ConsumerService.Db
{
    public interface IDbConfiguration
    {
        Task SaveMessageToDatabaseAsync(MessageModel messageModel);
    }
}