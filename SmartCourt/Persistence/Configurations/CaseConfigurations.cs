using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Entities;

namespace SmartCourt.Persistence.Configurations
{
    public class CaseConfigurations : IEntityTypeConfiguration<Case>
    {
        public void Configure(EntityTypeBuilder<Case> builder)
        {
            builder.HasQueryFilter(c => !c.IsDeleted);

            builder.HasOne(c => c.ClientProfile)
                .WithMany(cp => cp.Cases)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.LawyerProfile)
                .WithMany()
                .HasForeignKey(c => c.LawyerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.LastReview)
                .WithMany()
                .HasForeignKey(c => c.LastReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Chat)
                .WithMany()
                .HasForeignKey(c => c.ChatId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(c => c.Governorate)
                .HasMaxLength(100);

            builder.Property(c => c.City)
                .HasMaxLength(100);

            builder.Property(c => c.Status)
                .HasConversion<byte>();
        }
    }
}
