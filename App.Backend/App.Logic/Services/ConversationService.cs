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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserService currentUser;

        public ConversationService(UserManager<AppUser> userManager, IRepository<Message> repository, IHttpContextAccessor httpContextAccessor, ICurrentUserService currentUser)
        {
            _userManager = userManager;
            this.repository = repository;
            _httpContextAccessor = httpContextAccessor;
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
                
            List<Message> messages = (await repository.GetAllAsync())
                .Where(m => (m.SenderId == inUser.Id && m.ReceiverId == targetUser.Id) || (m.SenderId == targetUser.Id && m.ReceiverId == inUser.Id))
                .OrderBy(m => m.SentAt)
                .ToList();

            var conversationDtos = messages.Select(m => new MessageBoxDto
            {
                Id = m.Id,
                Content = m.Content,
                SentAt = m.SentAt,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId
            }).ToList();
        
            return Result<List<MessageBoxDto>>.Success(conversationDtos, 200);
        }
    }
}