namespace App.Core.Entities
{
    public class GroupMessage
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string? SenderUserNumber { get; set; }
        public string? Content { get; set; }
        public DateTime SentAt { get; set; }
        // Navigation
        public Group? Group { get; set; }
        public AppUser? SenderUser { get; set; }
    }
}
