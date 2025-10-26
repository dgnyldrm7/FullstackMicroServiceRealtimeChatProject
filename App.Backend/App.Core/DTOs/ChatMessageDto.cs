namespace App.Core.DTOs
{
    public class ChatMessageDto
    {
        public string SenderNumber { get; set; } = default!;
        public string ReceiverNumber { get; set; } = default!;
        public string? Content { get; set; }
        public DateTime SentAt { get; set; } = default!;
    }
}
