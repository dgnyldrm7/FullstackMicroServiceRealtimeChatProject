using App.Core.DTOs;
using App.Core.Entities;
using App.Core.Interface;
using App.Core.Result;
using App.Persistance.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace App.Logic.Services
{
    public class FriendService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly MessageRepository messageRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserService currentUser;

        public FriendService(UserManager<AppUser> userManager, MessageRepository messageRepository, IHttpContextAccessor httpContextAccessor, ICurrentUserService currentUser)
        {
            _userManager = userManager;
            this.messageRepository = messageRepository;
            _httpContextAccessor = httpContextAccessor;
            this.currentUser = currentUser;
        }

        public async Task<Result<List<FriendWithLastMessageDto>>> GetFriendsAsync()
        {
            AppUser? loggedInUser = await currentUser.GetLoggedInUserAsync();

            if (loggedInUser == null)
            {
                return Result<List<FriendWithLastMessageDto>>.Failure("User is not authenticated.", 401);
            }

            var InUser = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == loggedInUser.PhoneNumber);

            if (InUser == null)
            {
                return Result<List<FriendWithLastMessageDto>>.Failure("User not found.", 404);
            }

            var messages = await messageRepository.GetMessagesWithUsersAsync(InUser.Id);

            if (!messages.Any())
            {
                return Result<List<FriendWithLastMessageDto>>.Success(new List<FriendWithLastMessageDto>(), 200);
            }

            var friends = messages
                .Select(m => m.SenderId == InUser.Id ? m.Receiver : m.Sender)
                .Where(f => f != null && f.Id != InUser.Id)
                .Distinct()
                .ToList();

            var friendDtos = friends.Select(friend =>
            {
                var lastMessage = messages
                    .Where(m =>(m.SenderId == InUser.Id && m.ReceiverId == friend!.Id) || (m.SenderId == friend!.Id && m.ReceiverId == InUser.Id))
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefault();

                string lastMessageSenderId = lastMessage!.SenderId!;

                return new FriendWithLastMessageDto
                {
                    UserName = friend!.UserName,
                    PhoneNumber = friend.PhoneNumber,
                    ProfilePictureUrl = friend.ProfilePictureUrl,
                    LastMessageSenderId = lastMessageSenderId,
                    LastMessage = lastMessage?.Content,
                    LastMessageSentAt = lastMessage?.SentAt
                };
            }).ToList();

            return Result<List<FriendWithLastMessageDto>>.Success(friendDtos, 200);
        }
    }
}