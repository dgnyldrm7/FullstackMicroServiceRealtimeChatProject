using Microsoft.AspNetCore.Identity;

namespace App.Core.Entities
{
    public class AppUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime? LastSeen { get; set; }

        // Navigation properties
        public List<Message>? SentMessages { get; set; }
        public List<Message>? ReceivedMessages { get; set; }
        public List<GroupMember>? GroupMemberships { get; set; }
        public List<Group>? CreatedGroups { get; set; }
    }
}