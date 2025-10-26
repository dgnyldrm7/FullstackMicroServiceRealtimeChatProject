using App.Core.DTOs;
using App.Core.Result;


namespace App.Core.Interface
{
    public interface IMessageService
    {
        Task<Result<MessageDto>> SendMessageAsync(SendMessageDto sendMessageDto);
    }
}
