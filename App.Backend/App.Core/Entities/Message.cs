namespace App.Core.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string? SenderId { get; set; }
        public AppUser? Sender { get; set; } // Gönderen kullanıcı
        public string? ReceiverId { get; set; }
        public AppUser? Receiver { get; set; } // Alıcı kullanıcı
        public string? Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsDeletedBySender { get; set; }
        public bool IsDeletedByReceiver { get; set; }
        public bool IsGroupMessage { get; set; } // Grup mesajı mı?
    }
}