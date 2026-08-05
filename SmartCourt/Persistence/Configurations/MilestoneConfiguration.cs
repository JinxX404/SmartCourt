using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Milestones.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    public void Configure(EntityTypeBuilder<Milestone> builder)
    {
        builder.ToTable("Milestones");
        builder.HasKey(milestone => milestone.Id);

        builder.Property(milestone => milestone.Title)
            .IsRequired()
            .Unicode(200);
        builder.Property(milestone => milestone.Description)
            .NullableUnicode(10_000);
        builder.Property(milestone => milestone.Amount)
            .IsRequired()
            .Money();
        builder.Property(milestone => milestone.Status)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(milestone => milestone.AutoAcceptJobId)
            .NullableUnicode(100);
        builder.Property(milestone => milestone.RejectionReason)
            .NullableUnicode(2_000);
        builder.Property(milestone => milestone.AcceptanceSource)
            .HasConversion<int>();
        builder.Property(milestone => milestone.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
        builder.Property(milestone => milestone.CreatedAt).Utc();
        builder.Property(milestone => milestone.UpdatedAt).Utc();
        builder.Property(milestone => milestone.DueDate).NullableUtc();
        builder.Property(milestone => milestone.AcceptedByClientAt).NullableUtc();
        builder.Property(milestone => milestone.AcceptedByLawyerAt).NullableUtc();
        builder.Property(milestone => milestone.ReadyForFundingAt).NullableUtc();
        builder.Property(milestone => milestone.FundedAt).NullableUtc();
        builder.Property(milestone => milestone.SubmittedAt).NullableUtc();
        builder.Property(milestone => milestone.AutoAcceptEligibleAt).NullableUtc();
        builder.Property(milestone => milestone.AcceptedAt).NullableUtc();
        builder.Property(milestone => milestone.HoldStartsAt).NullableUtc();
        builder.Property(milestone => milestone.HoldExpiresAt).NullableUtc();
        builder.Property(milestone => milestone.ReleasedAt).NullableUtc();
        builder.Property(milestone => milestone.RefundedAt).NullableUtc();

        builder.HasOne<SmartCourt.Features.Contracts.Entities.Contract>()
            .WithMany()
            .HasForeignKey(milestone => milestone.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(milestone => new
        {
            milestone.ContractId,
            milestone.OrderNumber
        })
        .IsUnique()
        .HasDatabaseName("UX_Milestones_ContractId_OrderNumber");
        builder.HasIndex(milestone => new
        {
            milestone.ContractId,
            milestone.Status
        })
        .HasDatabaseName("IX_Milestones_ContractId_Status");
        builder.HasIndex(milestone => new
        {
            milestone.Status,
            milestone.AutoAcceptEligibleAt
        })
        .HasDatabaseName("IX_Milestones_Status_AutoAcceptEligibleAt");

        builder.HasCheckConstraint(
            "CK_Milestones_OrderNumber_Positive",
            "[OrderNumber] > 0");
        builder.HasCheckConstraint(
            "CK_Milestones_Amount_Positive",
            "[Amount] > 0");
        builder.HasCheckConstraint(
            "CK_Milestones_DurationDays_Range",
            "[DurationDays] IS NULL OR [DurationDays] BETWEEN 1 AND 365");
        builder.HasCheckConstraint(
            "CK_Milestones_Status_Range",
            "[Status] BETWEEN 0 AND 9");
        builder.HasCheckConstraint(
            "CK_Milestones_SubmissionVersion_Positive",
            "[SubmissionVersion] >= 0");
    }
}
