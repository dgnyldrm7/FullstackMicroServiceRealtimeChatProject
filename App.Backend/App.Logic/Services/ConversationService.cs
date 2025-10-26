using App.Core.DTOs;
using App.Core.Entities;
using App.Core.Interface;
using App.Core.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace App.Logic.Services
{
    public class ConversationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Message> repository;
        private readonly ICurrentUserService currentUser;

        public ConversationService(UserManager<AppUser> userManager, IRepository<Message> repository, ICurrentUserService currentUser)
        {
            _userManager = userManager;
            this.repository = repository;
            this.currentUser = currentUser;
        }

        public async Task<Result<List<MessageBoxDto>>> GetConversationAsync(string? targetNumber)
        {
            AppUser? loggedInUser = await currentUser.GetLoggedInUserAsync();

            if (loggedInUser == null)
            {
                return Result<List<MessageBoxDto>>.Failure("User is not authenticated.", 401);
            }

            if (string.IsNullOrWhiteSpace(targetNumber))
            {
                return Result<List<MessageBoxDto>>.Failure("Target number is required.", 404);
            }

            AppUser? inUser = loggedInUser;

            AppUser? targetUser = await _userManager.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == targetNumber);

            if (inUser == null || targetUser == null)
            {
                return Result<List<MessageBoxDto>>.Failure("User or users not found", 404);
            }

            var messages = await repository.Query()
                .Where(m =>
                    (m.SenderId == inUser.Id && m.ReceiverId == targetUser.Id) ||
                    (m.SenderId == targetUser.Id && m.ReceiverId == inUser.Id))
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var conversationDtos = messages.Select(m => new MessageBoxDto
            {
                Id = m.Id,
                Content = m.Content,
                SentAt = m.SentAt,
                SenderId = m.SenderId,
                SenderNumber = m.Sender?.PhoneNumber,
                ReceiverId = m.ReceiverId,
                ReceiverNumber = m.Receiver?.PhoneNumber
            }).ToList();

            return Result<List<MessageBoxDto>>.Success(conversationDtos, 200);
        }

        public async Task<Result<List<MessageBoxDto>>> GetAllConversationsAsync()
        {
            AppUser? loggedInUser = await currentUser.GetLoggedInUserAsync();

            if (loggedInUser == null) return Result<List<MessageBoxDto>>.Failure("User is not authenticated.", 401);

            var messages = await repository.Query()
                .Where(m => m.SenderId == loggedInUser.Id || m.ReceiverId == loggedInUser.Id)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var dtos = messages.Select(m => new MessageBoxDto
            {
                Id = m.Id,
                SenderNumber = m.Sender?.PhoneNumber,
                ReceiverNumber = m.Receiver?.PhoneNumber,
                Content = m.Content,
                SentAt = m.SentAt,
                ReceiverId = m.ReceiverId,
                SenderId = m.SenderId
            }).ToList();

            return Result<List<MessageBoxDto>>.Success(dtos, 200);
        }

    }
}
