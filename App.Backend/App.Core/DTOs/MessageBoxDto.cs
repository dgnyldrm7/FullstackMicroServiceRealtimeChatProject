namespace App.Core.DTOs
{
    public class MessageBoxDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public DateTime SentAt { get; set; }
    }
}
