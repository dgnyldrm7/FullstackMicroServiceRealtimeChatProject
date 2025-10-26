namespace App.Core.DTOs
{
    public class MessageDtoForRabbitMQ
    {
        public string SenderId { get; set; }
        public string SenderNumber { get; set; } = string.Empty;
        public string ReceiverNumber { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
    }
}
