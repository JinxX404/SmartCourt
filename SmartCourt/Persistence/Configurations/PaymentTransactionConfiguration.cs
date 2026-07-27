using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Payments.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class PaymentTransactionConfiguration
    : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");
        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.OperationType)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(transaction => transaction.ProviderName)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(100);
        builder.Property(transaction => transaction.ProviderTransactionId)
            .IsUnicode(false)
            .HasMaxLength(200);
        builder.Property(transaction => transaction.IdempotencyKey)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(200);
        builder.Property(transaction => transaction.Amount).Money();
        builder.Property(transaction => transaction.Currency)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(3)
            .HasDefaultValue("EGP");
        builder.Property(transaction => transaction.Status)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(transaction => transaction.FailureReason)
            .NullableUnicode(2_000);
        builder.Property(transaction => transaction.ProcessedAt).NullableUtc();
        builder.Property(transaction => transaction.CreatedAt).Utc();
        builder.Property(transaction => transaction.UpdatedAt).Utc();
        builder.Property(transaction => transaction.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne<SmartCourt.Features.Contracts.Entities.Contract>()
            .WithMany()
            .HasForeignKey(transaction => transaction.ContractId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SmartCourt.Features.Milestones.Entities.Milestone>()
            .WithMany()
            .HasForeignKey(transaction => transaction.MilestoneId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EscrowHold>()
            .WithMany()
            .HasForeignKey(transaction => transaction.EscrowHoldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(transaction => transaction.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("UX_PaymentTransactions_IdempotencyKey");
        builder.HasIndex(transaction => new
        {
            transaction.ProviderName,
            transaction.ProviderTransactionId
        })
        .IsUnique()
        .HasFilter("[ProviderTransactionId] IS NOT NULL")
        .HasDatabaseName("UX_PaymentTransactions_ProviderTransaction");
        builder.HasIndex(transaction => new
        {
            transaction.MilestoneId,
            transaction.Status
        })
        .HasDatabaseName("IX_PaymentTransactions_MilestoneId_Status");
        builder.HasIndex(transaction => new
        {
            transaction.ContractId,
            transaction.Status
        })
        .HasDatabaseName("IX_PaymentTransactions_ContractId_Status");

        builder.HasCheckConstraint(
            "CK_PaymentTransactions_Amount_Positive",
            "[Amount] > 0");
        builder.HasCheckConstraint(
            "CK_PaymentTransactions_Currency_EGP",
            "[Currency] = 'EGP'");
        builder.HasCheckConstraint(
            "CK_PaymentTransactions_OperationType_Range",
            "[OperationType] BETWEEN 0 AND 3");
        builder.HasCheckConstraint(
            "CK_PaymentTransactions_Status_Range",
            "[Status] BETWEEN 0 AND 2");
        builder.HasCheckConstraint(
            "CK_PaymentTransactions_MilestoneRequiredForMoneyOperations",
            "[OperationType] = 3 OR [MilestoneId] IS NOT NULL");
        builder.HasCheckConstraint(
            "CK_PaymentTransactions_CompletedDepositRequiresHold",
            "NOT ([OperationType] = 0 AND [Status] = 1) "
            + "OR [EscrowHoldId] IS NOT NULL");
    }
}
