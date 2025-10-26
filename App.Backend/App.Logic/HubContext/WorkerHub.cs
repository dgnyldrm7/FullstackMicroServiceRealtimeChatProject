using Microsoft.AspNetCore.SignalR;

namespace App.Logic.HubContext
{
    public class WorkerHub : Hub<IWorkerHub>
    {
        public async Task SendAllClient(string message)
        {
            await Clients.All.SendAllClient(message);
        }

        public async Task SendMessage(string receiverNumber, string message)
        {
            await Clients.User(receiverNumber).SendMessage(receiverNumber, message);
        }

        public async Task UpdateMessageList(string receiverNumber)
        {
            await Clients.User(receiverNumber).UpdateNotifyClientMessageList(receiverNumber);
        }
        /*
        //Sistemdeki giriş yapmış kullanıcı işlemleri
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }


        //Sistemdeki çıkış yapmış kullanıcı işlemleri
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
        */
    }
}
