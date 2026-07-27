using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Payments.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class WithdrawalRequestConfiguration
    : IEntityTypeConfiguration<WithdrawalRequest>
{
    public void Configure(EntityTypeBuilder<WithdrawalRequest> builder)
    {
        builder.ToTable("WithdrawalRequests");
        builder.HasKey(request => request.Id);

        builder.Property(request => request.Amount).Money();
        builder.Property(request => request.Currency)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(3)
            .HasDefaultValue("EGP");
        builder.Property(request => request.Status)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(request => request.ProviderTransactionId)
            .IsUnicode(false)
            .HasMaxLength(200);
        builder.Property(request => request.FailureReason)
            .NullableUnicode(2_000);
        builder.Property(request => request.IdempotencyKey)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(200);
        builder.Property(request => request.RequestedAt).Utc();
        builder.Property(request => request.ProcessedAt).NullableUtc();
        builder.Property(request => request.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(request => request.LawyerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(request => request.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("UX_WithdrawalRequests_IdempotencyKey");
        builder.HasIndex(request => new
        {
            request.LawyerUserId,
            request.Status
        })
        .HasDatabaseName("IX_WithdrawalRequests_LawyerUserId_Status");
        builder.HasCheckConstraint(
            "CK_WithdrawalRequests_Amount_Positive",
            "[Amount] > 0");
        builder.HasCheckConstraint(
            "CK_WithdrawalRequests_Currency_EGP",
            "[Currency] = 'EGP'");
        builder.HasCheckConstraint(
            "CK_WithdrawalRequests_Status_Range",
            "[Status] BETWEEN 0 AND 2");
    }
}
