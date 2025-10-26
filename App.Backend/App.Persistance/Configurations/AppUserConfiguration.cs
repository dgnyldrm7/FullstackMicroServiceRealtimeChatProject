using App.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Persistance.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(x => x.DisplayName)
               .HasMaxLength(100);

            builder.Property(x => x.ProfilePictureUrl)
                   .HasMaxLength(255);

            builder.HasMany(x => x.SentMessages)
                   .WithOne(x => x.Sender)
                   .HasForeignKey(x => x.SenderId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.ReceivedMessages)
                   .WithOne(x => x.Receiver)
                   .HasForeignKey(x => x.ReceiverId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.CreatedGroups)
                   .WithOne(x => x.CreatedByUser)
                   .HasForeignKey(x => x.AppUserId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.GroupMemberships)
                   .WithOne(x => x.User)
                   .HasForeignKey(x => x.UserNumber)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
