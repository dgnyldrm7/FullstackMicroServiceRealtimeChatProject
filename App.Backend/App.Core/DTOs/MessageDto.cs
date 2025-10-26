namespace App.Core.DTOs
{
    public class MessageDto
    {
        public string? Message { get; set; }
        public string? SenderPhoneNumber { get; set; }
        public string? ReceiverPhoneNumber { get; set; }
        public DateTime SentAt { get; set; }
    }
}