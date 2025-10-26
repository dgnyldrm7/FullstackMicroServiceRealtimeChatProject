using App.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Persistance.Configurations
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(100);

            builder.Property(x => x.Description)
                   .HasMaxLength(500);

            builder.Property(x => x.ProfilePictureUrl)
                   .HasMaxLength(255);

            builder.HasMany(x => x.GroupMembers)
                   .WithOne(x => x.Group)
                   .HasForeignKey(x => x.GroupId);

            builder.HasMany(x => x.GroupMessages)
                   .WithOne(x => x.Group)
                   .HasForeignKey(x => x.GroupId);
        }
    }
}
