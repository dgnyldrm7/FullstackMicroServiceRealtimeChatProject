using App.Core.DTOs;
using App.Core.Interface;
using App.Logic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace App.Logic.HubContext
{
    [Authorize]
    public class WorkerHub : Hub<IWorkerHub>
    {

        private readonly IUnitOfWork unitOfWork;

        private readonly ICurrentUserService currentUserService;

        private readonly ConversationService conversationService;

        private readonly ILogger<WorkerHub> logger;

        public WorkerHub(IUnitOfWork unitOfWork, ConversationService conversationService, ICurrentUserService currentUserService, ILogger<WorkerHub> logger)
        {
            this.unitOfWork = unitOfWork;
            this.conversationService = conversationService;
            this.currentUserService = currentUserService;
            this.logger = logger;
        }

        public async Task SendMessageFromWorker(ChatMessageDto dto)
        {
            await Clients.User(dto.SenderNumber).ReceiveMessageAsync(dto);

            await Clients.User(dto.ReceiverNumber).ReceiveMessageAsync(dto);

            await Clients.User(dto.SenderNumber)
                .UpdateNotifyClientMessageList(dto.SenderNumber, dto.ReceiverNumber);
            
            await Clients.User(dto.ReceiverNumber)
                .UpdateNotifyClientMessageList(dto.SenderNumber, dto.ReceiverNumber);
        }

        public async Task UpdateMessageList(string senderNumber, string receiverNumber)
        {
            await Clients.User(receiverNumber)
                .UpdateNotifyClientMessageList(senderNumber, receiverNumber);
        }

        public async Task UserTyping(string receiverNumber)
        {
            string? senderNumber = Context.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.MobilePhone)?.Value;
            
            if (!string.IsNullOrEmpty(senderNumber))
            {
                await Clients.User(receiverNumber).UserTyping(senderNumber);
            }
        }
        
        public override async Task OnConnectedAsync()
        {

            await base.OnConnectedAsync();
        }
        
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}