using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Payments.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class EscrowLedgerEntryConfiguration
    : IEntityTypeConfiguration<EscrowLedgerEntry>
{
    public void Configure(EntityTypeBuilder<EscrowLedgerEntry> builder)
    {
        builder.ToTable("EscrowLedgerEntries");
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.TransactionType)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(entry => entry.Amount).Money();
        builder.Property(entry => entry.RunningBalance).Money();
        builder.Property(entry => entry.Currency)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(3)
            .HasDefaultValue("EGP");
        builder.Property(entry => entry.ReferenceType)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(100);
        builder.Property(entry => entry.Description)
            .IsRequired()
            .Unicode(2_000);
        builder.Property(entry => entry.CreatedAt).Utc();

        builder.HasOne<EscrowAccount>()
            .WithMany()
            .HasForeignKey(entry => entry.EscrowAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EscrowHold>()
            .WithMany()
            .HasForeignKey(entry => entry.EscrowHoldId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PaymentTransaction>()
            .WithMany()
            .HasForeignKey(entry => entry.PaymentTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(entry => entry.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entry => new
        {
            entry.EscrowAccountId,
            entry.CreatedAt
        })
        .HasDatabaseName("IX_EscrowLedgerEntries_AccountId_CreatedAt");
        builder.HasCheckConstraint(
            "CK_EscrowLedgerEntries_Amount_Positive",
            "[Amount] > 0");
        builder.HasCheckConstraint(
            "CK_EscrowLedgerEntries_RunningBalance_NonNegative",
            "[RunningBalance] >= 0");
        builder.HasCheckConstraint(
            "CK_EscrowLedgerEntries_Currency_EGP",
            "[Currency] = 'EGP'");
        builder.HasCheckConstraint(
            "CK_EscrowLedgerEntries_TransactionType_Range",
            "[TransactionType] BETWEEN 0 AND 4");
    }
}
