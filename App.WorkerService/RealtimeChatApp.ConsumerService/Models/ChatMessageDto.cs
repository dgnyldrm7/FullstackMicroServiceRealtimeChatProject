namespace RealtimeChatApp.ConsumerService.Models
{
    public class ChatMessageDto
    {
        public string SenderNumber { get; set; } = default!;
        public string ReceiverNumber { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTime SentAt { get; set; }
    }
}
