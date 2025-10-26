using App.Core.DTOs;
using App.Core.Entities;
using App.Core.Interface;
using App.Core.Interface.RabbitMQ;
using App.Core.Result;
using App.Logic.HubContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace App.Logic.Services
{
    public class MessageService : IMessageService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICurrentUserService currentUser;
        private readonly IRabbitMQPublisher rabbitMQPublisher;
        private readonly IHubContext<WorkerHub, IWorkerHub> _hubContext;
        public MessageService(UserManager<AppUser> userManager, ICurrentUserService currentUser, IRabbitMQPublisher rabbitMQPublisher, IHubContext<WorkerHub, IWorkerHub> hubContext)
        {
            _userManager = userManager;
            this.currentUser = currentUser;
            this.rabbitMQPublisher = rabbitMQPublisher;
            _hubContext = hubContext;
        }

        public async Task<Result<MessageDto>> SendMessageAsync(ChatMessageDto dto)
        {
            AppUser? loggedInUser = await currentUser.GetLoggedInUserAsync();

            if (loggedInUser == null)
            {
                return Result<MessageDto>.Failure("User is not authenticated", 401);
            }

            AppUser? sender = loggedInUser;

            if (sender == null)
            {
                return Result<MessageDto>.Failure("Sender not found", 404);
            }
                
            AppUser? receiver = await _userManager.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == dto.ReceiverNumber);

            if (receiver == null)
            {
                return Result<MessageDto>.Failure("Receiver not found", 404);
            }

            var message = new Message
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Content = dto.Content,
                SentAt = DateTime.UtcNow
            };

            await rabbitMQPublisher.Publish(
            new MessageDtoForRabbitMQ
            {
                SenderId = sender.Id,
                SenderNumber = sender.PhoneNumber,
                ReceiverId = receiver.Id,
                ReceiverNumber = receiver.PhoneNumber,
                Content = dto.Content,
                SentAt = message.SentAt
            });

            var messageDto = new MessageDto
            {
                Message = dto.Content,
                SentAt = DateTime.UtcNow,
                SenderPhoneNumber = sender.PhoneNumber,
                ReceiverPhoneNumber = receiver.PhoneNumber
            };

            return Result<MessageDto>.Success(messageDto, 200);
        }

        
    }
}