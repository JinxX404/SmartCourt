using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Payments.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class EscrowHoldConfiguration : IEntityTypeConfiguration<EscrowHold>
{
    public void Configure(EntityTypeBuilder<EscrowHold> builder)
    {
        builder.ToTable("EscrowHolds");
        builder.HasKey(hold => hold.Id);

        builder.Property(hold => hold.GrossAmount).Money();
        builder.Property(hold => hold.PlatformFeeAmount).Money();
        builder.Property(hold => hold.NetAmount).Money();
        builder.Property(hold => hold.Status)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(hold => hold.SettlementType)
            .HasConversion<int>();
        builder.Property(hold => hold.FundedAt).Utc();
        builder.Property(hold => hold.HoldStartsAt).NullableUtc();
        builder.Property(hold => hold.HoldExpiresAt).NullableUtc();
        builder.Property(hold => hold.FrozenAt).NullableUtc();
        builder.Property(hold => hold.SettledAt).NullableUtc();
        builder.Property(hold => hold.CreatedAt).Utc();
        builder.Property(hold => hold.UpdatedAt).Utc();
        builder.Property(hold => hold.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne<EscrowAccount>()
            .WithMany()
            .HasForeignKey(hold => hold.EscrowAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SmartCourt.Features.Contracts.Entities.Contract>()
            .WithMany()
            .HasForeignKey(hold => hold.ContractId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SmartCourt.Features.Milestones.Entities.Milestone>()
            .WithMany()
            .HasForeignKey(hold => hold.MilestoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(hold => hold.MilestoneId)
            .IsUnique()
            .HasDatabaseName("UX_EscrowHolds_MilestoneId");
        builder.HasIndex(hold => new
        {
            hold.HoldExpiresAt,
            hold.Status
        })
        .HasDatabaseName("IX_EscrowHolds_HoldExpiresAt_Status");
        builder.HasCheckConstraint(
            "CK_EscrowHolds_GrossAmount_Positive",
            "[GrossAmount] > 0");
        builder.HasCheckConstraint(
            "CK_EscrowHolds_FeesAndNet_NonNegative",
            "[PlatformFeeAmount] >= 0 AND [NetAmount] >= 0");
        builder.HasCheckConstraint(
            "CK_EscrowHolds_Reconciliation",
            "[GrossAmount] = [PlatformFeeAmount] + [NetAmount]");
        builder.HasCheckConstraint(
            "CK_EscrowHolds_Status_Range",
            "[Status] BETWEEN 0 AND 3");
        builder.HasCheckConstraint(
            "CK_EscrowHolds_FundedStateRequiresTimestamp",
            "[Status] <> 0 OR [FundedAt] IS NOT NULL");
    }
}
