using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Milestones.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class MilestoneStateHistoryConfiguration
    : IEntityTypeConfiguration<MilestoneStateHistory>
{
    public void Configure(EntityTypeBuilder<MilestoneStateHistory> builder)
    {
        builder.ToTable("MilestoneStateHistories");
        builder.HasKey(history => history.Id);
        builder.Property(history => history.PreviousStatus)
            .HasConversion<int>();
        builder.Property(history => history.NewStatus)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(history => history.Trigger)
            .IsRequired()
            .Unicode(100);
        builder.Property(history => history.Reason)
            .NullableUnicode(2_000);
        builder.Property(history => history.CreatedAt).Utc();

        builder.HasOne<Milestone>()
            .WithMany()
            .HasForeignKey(history => history.MilestoneId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(history => history.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(history => new
        {
            history.MilestoneId,
            history.CreatedAt
        })
        .HasDatabaseName("IX_MilestoneStateHistories_MilestoneId_CreatedAt");
        builder.HasCheckConstraint(
            "CK_MilestoneStateHistories_NewStatus_Range",
            "[NewStatus] BETWEEN 0 AND 9");
        builder.HasCheckConstraint(
            "CK_MilestoneStateHistories_PreviousStatus_Range",
            "[PreviousStatus] IS NULL OR [PreviousStatus] BETWEEN 0 AND 9");
    }
}
