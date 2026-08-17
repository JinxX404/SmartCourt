using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Disputes.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class DisputeConfiguration : IEntityTypeConfiguration<Dispute>
{
    public void Configure(EntityTypeBuilder<Dispute> builder)
    {
        builder.ToTable("Disputes");
        builder.HasKey(dispute => dispute.Id);

        builder.Property(dispute => dispute.Category)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(dispute => dispute.Title)
            .IsRequired()
            .Unicode(200);
        builder.Property(dispute => dispute.Description)
            .IsRequired()
            .Unicode(20_000);
        builder.Property(dispute => dispute.Status)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(dispute => dispute.RequestedOutcome)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(dispute => dispute.ResolutionType)
            .HasConversion<int>();
        builder.Property(dispute => dispute.ResolutionAmount).Money();
        builder.Property(dispute => dispute.ResolutionSummary)
            .NullableUnicode(2_000);
        builder.Property(dispute => dispute.ResolvedAt).NullableUtc();
        builder.Property(dispute => dispute.ClosedAt).NullableUtc();
        builder.Property(dispute => dispute.PreviousMilestoneStatus)
            .HasConversion<int>();
        builder.Property(dispute => dispute.PreviousContractStatus)
            .HasConversion<int>();
        builder.Property(dispute => dispute.CancelledAt).NullableUtc();
        builder.Property(dispute => dispute.CancellationReason)
            .NullableUnicode(2_000);
        builder.Property(dispute => dispute.CreatedAt).Utc();
        builder.Property(dispute => dispute.UpdatedAt).Utc();
        builder.Property(dispute => dispute.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne<SmartCourt.Features.Contracts.Entities.Contract>()
            .WithMany()
            .HasForeignKey(dispute => dispute.ContractId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SmartCourt.Features.Milestones.Entities.Milestone>()
            .WithMany()
            .HasForeignKey(dispute => dispute.MilestoneId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(dispute => dispute.RaisedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(dispute => dispute.AssignedModeratorUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(dispute => dispute.ResolvedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(dispute => dispute.CancelledByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(dispute => dispute.MilestoneId)
        .HasFilter("[Status] IN (0, 1, 2)")
        .IsUnique()
        .HasDatabaseName("UX_Disputes_OpenPerMilestone");
        builder.HasIndex(dispute => new
        {
            dispute.Status,
            dispute.CreatedAt
        })
        .HasDatabaseName("IX_Disputes_Status_CreatedAt");
        builder.HasCheckConstraint(
            "CK_Disputes_Category_Range",
            "[Category] BETWEEN 0 AND 5");
        builder.HasCheckConstraint(
            "CK_Disputes_Status_Range",
            "[Status] BETWEEN 0 AND 5");
        builder.HasCheckConstraint(
            "CK_Disputes_RequestedOutcome_Range",
            "[RequestedOutcome] BETWEEN 0 AND 2");
        builder.HasCheckConstraint(
            "CK_Disputes_ResolutionType_Range",
            "[ResolutionType] IS NULL OR [ResolutionType] BETWEEN 0 AND 2");
    }
}
