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
        Task UpdateNotifyClientMessageList(string userNumber);

        /// <summary>
        /// İlgili receiver'i belirli bir kullanıcıya mesaj gönderir.
        /// </summary>
        Task SendMessage(string receiverNumber, string message);
    }
}