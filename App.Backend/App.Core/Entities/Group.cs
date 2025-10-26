namespace App.Core.Entities
{
    public class Group
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        //Navigation Property
        public string? AppUserId { get; set; }
        public AppUser? CreatedByUser { get; set; }
        public List<GroupMember>? GroupMembers { get; set; }
        public List<GroupMessage>? GroupMessages { get; set; }
    }
}