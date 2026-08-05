using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Milestones.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class MilestoneSubmissionConfiguration
    : IEntityTypeConfiguration<MilestoneSubmission>
{
    public void Configure(EntityTypeBuilder<MilestoneSubmission> builder)
    {
        builder.ToTable("MilestoneSubmissions");
        builder.HasKey(submission => submission.Id);

        builder.Property(submission => submission.Notes)
            .IsRequired()
            .Unicode(10_000);
        builder.Property(submission => submission.SubmittedAt).Utc();

        builder.HasOne<Milestone>()
            .WithMany()
            .HasForeignKey(submission => submission.MilestoneId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SmartCourt.Features.Payments.Entities.EscrowHold>()
            .WithMany()
            .HasForeignKey(submission => submission.EscrowHoldId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(submission => submission.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(submission => new
        {
            submission.MilestoneId,
            submission.Version
        })
        .IsUnique()
        .HasDatabaseName("UX_MilestoneSubmissions_MilestoneId_Version");
        builder.HasCheckConstraint(
            "CK_MilestoneSubmissions_Version_Positive",
            "[Version] > 0");
    }
}
