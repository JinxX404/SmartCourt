using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.LawyerSubscription.Entities;

namespace SmartCourt.Features.LawyerSubscription.Persistence;

internal sealed class LawyerPaymentTransactionConfiguration : IEntityTypeConfiguration<LawyerPaymentTransaction>
{
    public void Configure(EntityTypeBuilder<LawyerPaymentTransaction> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LawyerId).IsRequired();
        builder.Property(x => x.TargetId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.TargetType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.PriceEgp).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ProviderName).IsRequired().HasMaxLength(50);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200);
        builder.HasIndex(x => new { x.ProviderName, x.IdempotencyKey }).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
        builder.HasIndex(x => x.ProviderTransactionId);
    }
}
