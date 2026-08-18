using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.ChatAgent.Entities;

namespace SmartCourt.Features.ChatAgent.Persistence;

public class TokenBundlePaymentTransactionConfiguration : IEntityTypeConfiguration<TokenBundlePaymentTransaction>
{
    public void Configure(EntityTypeBuilder<TokenBundlePaymentTransaction> builder)
    {
        builder.ToTable("TokenBundlePaymentTransactions");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ProviderName).HasMaxLength(100);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200);
        builder.Property(x => x.ProviderTransactionId).HasMaxLength(200);
        builder.Property(x => x.RelatedProviderTransactionId).HasMaxLength(200);
        builder.Property(x => x.ProviderStatus).HasMaxLength(100);
        builder.Property(x => x.FailureReason).HasMaxLength(1000);
        builder.Property(x => x.BundleId).HasMaxLength(100);

        builder.Property(x => x.PriceEgp).HasPrecision(18, 4);

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(x => new { x.ProviderName, x.IdempotencyKey })
            .IsUnique();
        
        builder.HasIndex(x => x.ProviderTransactionId);
    }
}
