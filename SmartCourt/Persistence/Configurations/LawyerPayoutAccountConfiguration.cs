using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Payments.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class LawyerPayoutAccountConfiguration
    : IEntityTypeConfiguration<LawyerPayoutAccount>
{
    public void Configure(EntityTypeBuilder<LawyerPayoutAccount> builder)
    {
        builder.ToTable("LawyerPayoutAccounts");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ProviderCode)
            .IsRequired().IsUnicode(false).HasMaxLength(100);
        builder.Property(item => item.ProviderAccountId)
            .IsRequired().IsUnicode(false).HasMaxLength(200);
        builder.Property(item => item.Status).IsRequired().HasConversion<int>();
        builder.Property(item => item.Country)
            .IsRequired().IsUnicode(false).HasMaxLength(2);
        builder.Property(item => item.DefaultCurrency)
            .IsRequired().IsUnicode(false).HasMaxLength(3);
        builder.Property(item => item.AvailableProviderAmountMinor)
            .IsRequired()
            .HasDefaultValue(0L);
        builder.Property(item => item.MaskedDestination).NullableUnicode(200);
        builder.Property(item => item.LastProviderStatus)
            .IsUnicode(false).HasMaxLength(100);
        builder.Property(item => item.LastProviderErrorCode)
            .IsUnicode(false).HasMaxLength(200);
        builder.Property(item => item.LastSynchronizedAt).NullableUtc();
        builder.Property(item => item.CreatedAt).Utc();
        builder.Property(item => item.UpdatedAt).Utc();
        builder.Property(item => item.RowVersion)
            .IsRowVersion().IsConcurrencyToken();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(item => item.LawyerUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.LawyerUserId, item.ProviderCode })
            .IsUnique()
            .HasDatabaseName("UX_LawyerPayoutAccounts_Lawyer_Provider");
        builder.HasIndex(item => new { item.ProviderCode, item.ProviderAccountId })
            .IsUnique()
            .HasDatabaseName("UX_LawyerPayoutAccounts_ProviderAccount");
        builder.HasCheckConstraint(
            "CK_LawyerPayoutAccounts_Status_Range",
            "[Status] BETWEEN 0 AND 4");
        builder.HasCheckConstraint(
            "CK_LawyerPayoutAccounts_ProviderBalance_NonNegative",
            "[AvailableProviderAmountMinor] >= 0");
    }
}
