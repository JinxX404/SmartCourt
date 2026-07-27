using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Payments.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class EscrowAccountConfiguration
    : IEntityTypeConfiguration<EscrowAccount>
{
    public void Configure(EntityTypeBuilder<EscrowAccount> builder)
    {
        builder.ToTable("EscrowAccounts");
        builder.HasKey(account => account.Id);

        builder.Property(account => account.Currency)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(3)
            .HasDefaultValue("EGP");
        builder.Property(account => account.TotalDeposited).Money();
        builder.Property(account => account.TotalReleased).Money();
        builder.Property(account => account.TotalRefunded).Money();
        builder.Property(account => account.TotalFees).Money();
        builder.Property(account => account.Status)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(account => account.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
        builder.Property(account => account.CreatedAt).Utc();
        builder.Property(account => account.UpdatedAt).Utc();

        builder.HasOne<SmartCourt.Features.Contracts.Entities.Contract>()
            .WithMany()
            .HasForeignKey(account => account.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(account => account.ContractId)
            .IsUnique()
            .HasDatabaseName("UX_EscrowAccounts_ContractId");
        builder.HasCheckConstraint(
            "CK_EscrowAccounts_Currency_EGP",
            "[Currency] = 'EGP'");
        builder.HasCheckConstraint(
            "CK_EscrowAccounts_Status_Range",
            "[Status] BETWEEN 0 AND 1");
        builder.HasCheckConstraint(
            "CK_EscrowAccounts_NonNegativeTotals",
            "[TotalDeposited] >= 0 AND [TotalReleased] >= 0 AND "
            + "[TotalRefunded] >= 0 AND [TotalFees] >= 0");
    }
}
