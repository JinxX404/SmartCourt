using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Entities;

namespace SmartCourt.Persistence.Configurations
{
    public class CaseConfigurations : IEntityTypeConfiguration<Case>
    {
        public void Configure(EntityTypeBuilder<Case> builder)
        {
            builder.HasOne(c => c.ClientProfile)
                .WithMany(cp => cp.Cases)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(c => c.Governorate)
                .HasMaxLength(100);

            builder.Property(c => c.City)
                .HasMaxLength(100);

            builder.Property(c => c.Status)
                .HasConversion<byte>();
        }
    }
}
