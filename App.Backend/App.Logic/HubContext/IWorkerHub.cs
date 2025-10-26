using App.Core.DTOs;

namespace App.Logic.HubContext
{
    public interface IWorkerHub
    {
        /// <summary>
        /// SendMessage is a method that sends a message to all connected clients.
        /// </summary>
        Task SendAllClient(string message);

        /// <summary>
        /// İlgili user'ın tüm mesasage list'ini anlık olarak güncellerç
        /// </summary>
        Task UpdateNotifyClientMessageList(string senderNumber, string receiverNumber);

        /// <summary>
        /// İlgili receiver'i belirli bir kullanıcıya mesaj gönderir.
        /// </summary>
        Task ReceiveMessageAsync(ChatMessageDto chatMessageDto);

        /// <summary>
        /// İlgili kullanıcının yazıyor ya da yazmıyor ifadesini gösterir.
        /// </summary>
        /// <param name="senderNumber"></param>
        /// <param name="receiverNumber"></param>
        /// <returns></returns>
        Task UserTyping(string senderNumber);
    }
}