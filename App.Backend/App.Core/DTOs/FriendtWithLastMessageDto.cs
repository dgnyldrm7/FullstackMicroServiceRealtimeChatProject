namespace App.Core.DTOs
{
    public class FriendWithLastMessageDto
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? LastMessageSenderId { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageSentAt { get; set; }
    }
}