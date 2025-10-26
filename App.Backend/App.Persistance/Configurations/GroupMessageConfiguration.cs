using App.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Persistance.Configurations
{
    public class GroupMessageConfiguration : IEntityTypeConfiguration<GroupMessage>
    {
        public void Configure(EntityTypeBuilder<GroupMessage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(x => x.SentAt)
                   .IsRequired();

            builder.HasOne(x => x.Group)
                   .WithMany(x => x.GroupMessages)
                   .HasForeignKey(x => x.GroupId);

            builder.HasOne(x => x.SenderUser)
                   .WithMany()
                   .HasForeignKey(x => x.SenderUserNumber)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
