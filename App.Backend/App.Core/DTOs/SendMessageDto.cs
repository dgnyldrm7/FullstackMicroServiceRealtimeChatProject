namespace App.Core.DTOs
{
    public class SendMessageDto
    {
        public string ReceiverUserNumber { get; set; } = default!;
        public string? Content { get; set; }
    }
}
