using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Entities;

namespace SmartCourt.Persistence.Configurations
{
    public class ReviewPointConfigurations : IEntityTypeConfiguration<ReviewPoint>
    {
        public void Configure(EntityTypeBuilder<ReviewPoint> builder)
        {
            builder.HasOne(rp => rp.CaseReviewReport)
                .WithMany(crr => crr.ReviewPoints)
                .HasForeignKey(rp => rp.CaseReviewReportId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
