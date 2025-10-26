using App.Core.DTOs;
using App.Core.Entities;
using App.Core.Interface;
using App.Core.Result;
using App.Logic.HubContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Logic.Services
{
    public class MessageService : IMessageService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Message> repository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserService currentUser;
        private readonly ILogger<MessageService> _logger;

        public MessageService(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IRepository<Message> repository, IHttpContextAccessor httpContextAccessor, ILogger<MessageService> logger, IHubContext<WorkerHub> hubContext, ICurrentUserService currentUser)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            this.repository = repository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            this.currentUser = currentUser;
        }

        public async Task<Result<MessageDto>> SendMessageAsync(SendMessageDto dto)
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
                .FirstOrDefaultAsync(x => x.PhoneNumber == dto.ReceiverUserNumber);

            if (receiver == null)
            {
                return Result<MessageDto>.Failure("Receiver not found", 404);
            }

            _logger.LogWarning($"receiver phone number: {receiver?.PhoneNumber}");

            Message messageData = new Message
            {
                SenderId = sender.Id,
                ReceiverId = receiver?.Id,
                Content = dto.Content,
                SentAt = DateTime.UtcNow.AddHours(3) //For turkey hours!
            };

            await repository.AddAsync(messageData);

            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            return Result<MessageDto>.Success(new MessageDto
            {
                Message = messageData.Content,
                SentAt = messageData.SentAt,
                SenderPhoneNumber = sender.PhoneNumber,
                ReceiverPhoneNumber = receiver?.PhoneNumber
            }, 200);
        }
    }
}