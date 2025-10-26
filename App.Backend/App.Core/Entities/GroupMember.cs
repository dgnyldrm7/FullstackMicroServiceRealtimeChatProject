namespace App.Core.Entities
{
    public class GroupMember
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string? UserNumber { get; set; }
        // Navigation properties
        public Group? Group { get; set; }
        public AppUser? User { get; set; }
    }
}
