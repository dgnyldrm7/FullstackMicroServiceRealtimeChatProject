using App.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Persistance.Configurations
{
    public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
    {
        public void Configure(EntityTypeBuilder<GroupMember> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.GroupId, x.UserNumber })
                   .IsUnique();

            builder.HasOne(x => x.Group)
                   .WithMany(x => x.GroupMembers)
                   .HasForeignKey(x => x.GroupId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                   .WithMany(x => x.GroupMemberships)
                   .HasForeignKey(x => x.UserNumber)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
