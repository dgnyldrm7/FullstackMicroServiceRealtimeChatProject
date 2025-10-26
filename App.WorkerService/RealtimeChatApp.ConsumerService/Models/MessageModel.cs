namespace RealtimeChatApp.ConsumerService.Models
{
    // Gelen JSON'u deserialize etmek için basit bir model
    public class MessageModel
    {
        public string SenderId { get; set; }
        public string SenderNumber { get; set; } = string.Empty;
        public string ReceiverId { get; set; }
        public string ReceiverNumber { get; set; } = string.Empty;
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
    }
}
