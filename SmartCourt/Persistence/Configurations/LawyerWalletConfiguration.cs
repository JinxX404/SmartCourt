using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Payments.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class LawyerWalletConfiguration
    : IEntityTypeConfiguration<LawyerWallet>
{
    public void Configure(EntityTypeBuilder<LawyerWallet> builder)
    {
        builder.ToTable("LawyerWallets");
        builder.HasKey(wallet => wallet.Id);

        builder.Property(wallet => wallet.Currency)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(3)
            .HasDefaultValue("EGP");
        builder.Property(wallet => wallet.PendingBalance).Money();
        builder.Property(wallet => wallet.AvailableBalance).Money();
        builder.Property(wallet => wallet.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
        builder.Property(wallet => wallet.CreatedAt).Utc();
        builder.Property(wallet => wallet.UpdatedAt).Utc();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(wallet => wallet.LawyerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(wallet => wallet.LawyerUserId)
            .IsUnique()
            .HasDatabaseName("UX_LawyerWallets_LawyerUserId");
        builder.HasCheckConstraint(
            "CK_LawyerWallets_Currency_EGP",
            "[Currency] = 'EGP'");
        builder.HasCheckConstraint(
            "CK_LawyerWallets_Balances_NonNegative",
            "[PendingBalance] >= 0 AND [AvailableBalance] >= 0");
    }
}
