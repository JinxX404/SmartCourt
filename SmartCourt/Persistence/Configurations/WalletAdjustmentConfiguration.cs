using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Common.Entities;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Payments.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class WalletAdjustmentConfiguration
    : IEntityTypeConfiguration<WalletAdjustment>
{
    public void Configure(EntityTypeBuilder<WalletAdjustment> builder)
    {
        builder.ToTable("WalletAdjustments");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.PendingBalanceDelta).Money();
        builder.Property(item => item.AvailableBalanceDelta).Money();
        builder.Property(item => item.PendingBalanceBefore).Money();
        builder.Property(item => item.PendingBalanceAfter).Money();
        builder.Property(item => item.AvailableBalanceBefore).Money();
        builder.Property(item => item.AvailableBalanceAfter).Money();
        builder.Property(item => item.Reason)
            .IsRequired()
            .Unicode(2_000);
        builder.Property(item => item.CreatedAt).Utc();

        builder.HasOne<LawyerWallet>()
            .WithMany()
            .HasForeignKey(item => item.LawyerWalletId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Contract>()
            .WithMany()
            .HasForeignKey(item => item.ContractId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EscrowAccount>()
            .WithMany()
            .HasForeignKey(item => item.EscrowAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EscrowLedgerEntry>()
            .WithOne()
            .HasForeignKey<WalletAdjustment>(item => item.LedgerEntryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(item => item.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => new
            {
                item.LawyerWalletId,
                item.CreatedAt
            })
            .HasDatabaseName("IX_WalletAdjustments_WalletId_CreatedAt");
        builder.HasCheckConstraint(
            "CK_WalletAdjustments_Delta_NonZero",
            "[PendingBalanceDelta] <> 0 OR [AvailableBalanceDelta] <> 0");
        builder.HasCheckConstraint(
            "CK_WalletAdjustments_Balances_NonNegative",
            "[PendingBalanceBefore] >= 0 AND [PendingBalanceAfter] >= 0 "
            + "AND [AvailableBalanceBefore] >= 0 AND [AvailableBalanceAfter] >= 0");
    }
}
