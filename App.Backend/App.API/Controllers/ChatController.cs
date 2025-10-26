using App.Core.DTOs;
using App.Core.Entities;
using App.Core.Interface.RabbitMQ;
using App.Core.Result;
using App.Infrastructure.RabbitMQ;
using App.Logic.HubContext;
using App.Logic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace App.API.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {     
        private readonly MessageService _messageService;
        private readonly ConversationService conversationService;
        private readonly FriendService friendService;
        private readonly IHubContext<WorkerHub, IWorkerHub> _hubContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(MessageService messageService, ConversationService conversationService, FriendService friendService, IHubContext<WorkerHub, IWorkerHub> hubContext, ILogger<ChatController> logger)
        {
            _messageService = messageService;
            this.conversationService = conversationService;
            this.friendService = friendService;
            _hubContext = hubContext;
            _hubContext = hubContext;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("messages")]
        [ProducesResponseType(typeof(Result<Message>), 200)]
        [ProducesResponseType(typeof(Result<Message>), 400)]
        public async Task<IActionResult> SendToMessage(ChatMessageDto chatMessageDto)
        {
            Result<MessageDto> messageBox = await _messageService.SendMessageAsync(chatMessageDto);

            if (!messageBox.IsSuccess || messageBox.Data == null)
            {
                return StatusCode(messageBox.StatusCode, messageBox);
            }            

            return StatusCode(messageBox.StatusCode, messageBox.Data);
        }

        [Authorize]
        [HttpGet("target-conversation/{targetNumber?}")]
        [ProducesResponseType(typeof(Result<List<MessageBoxDto>>), 404)]
        [ProducesResponseType(typeof(Result<List<MessageBoxDto>>), 401)]
        [ProducesResponseType(typeof(Result<List<MessageBoxDto>>), 200)]
        public async Task<IActionResult> GetConversation(string targetNumber)
        {
            Result<List<MessageBoxDto>> conversation = await conversationService.GetConversationAsync(targetNumber);
            
            return StatusCode(conversation.StatusCode, conversation);
        }

        [Authorize]
        [HttpGet("all-conversations")]
        [ProducesResponseType(typeof(Result<List<MessageBoxDto>>), 404)]
        [ProducesResponseType(typeof(Result<List<MessageBoxDto>>), 401)]
        [ProducesResponseType(typeof(Result<List<MessageBoxDto>>), 200)]
        public async Task<IActionResult> GetAllConversations()
        {
            Result<List<MessageBoxDto>> conversation = await conversationService.GetAllConversationsAsync();

            return StatusCode(conversation.StatusCode, conversation);
        }

        [Authorize]
        [HttpGet("friends")]
        [ProducesResponseType(typeof(Result<List<FriendWithLastMessageDto>>), 200)]
        [ProducesResponseType(typeof(Result<List<FriendWithLastMessageDto>>), 404)]
        [ProducesResponseType(typeof(Result<List<FriendWithLastMessageDto>>), 401)]
        public async Task<IActionResult> GetFriends()
        {
            Result<List<FriendWithLastMessageDto>> friends = await friendService.GetFriendsAsync();

            return StatusCode(friends.StatusCode, friends);
        }
    }
}