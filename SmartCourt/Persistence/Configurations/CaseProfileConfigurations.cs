using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Entities;

namespace SmartCourt.Persistence.Configurations
{
    public class CaseProfileConfigurations : IEntityTypeConfiguration<CaseProfile>
    {
        public void Configure(EntityTypeBuilder<CaseProfile> builder)
        {
            builder.HasOne(cp => cp.Case)
                .WithOne(c => c.CaseProfile)
                .HasForeignKey<CaseProfile>(cp => cp.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(cp => cp.Specialization)
                .HasConversion<int>();

            builder.Property(cp => cp.RequiredLawyerLevelId)
                .HasConversion<int>();

            builder.Property(cp => cp.Complexity)
                .HasConversion<int>();
        }
    }
}
