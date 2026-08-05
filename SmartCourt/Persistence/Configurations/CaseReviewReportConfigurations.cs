using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Entities;

namespace SmartCourt.Persistence.Configurations
{
    public class CaseReviewReportConfigurations : IEntityTypeConfiguration<CaseReviewReport>
    {
        public void Configure(EntityTypeBuilder<CaseReviewReport> builder)
        {
            builder.HasOne(crr => crr.Case)
                .WithMany(c => c.ReviewReports)
                .HasForeignKey(crr => crr.CaseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
