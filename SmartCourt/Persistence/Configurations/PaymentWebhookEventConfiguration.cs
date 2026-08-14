using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Payments.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class PaymentWebhookEventConfiguration
    : IEntityTypeConfiguration<PaymentWebhookEvent>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookEvent> builder)
    {
        builder.ToTable("PaymentWebhookEvents");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.EventId)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(200);
        builder.Property(item => item.ProviderCode)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(50);
        builder.Property(item => item.EventType)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(100);
        builder.Property(item => item.ProviderObjectId)
            .IsUnicode(false)
            .HasMaxLength(200);
        builder.Property(item => item.ConnectedAccountId)
            .IsUnicode(false)
            .HasMaxLength(200);
        builder.Property(item => item.ReceivedAt).Utc();
        builder.Property(item => item.ProcessedAt);
        builder.Property(item => item.ProcessingError).HasMaxLength(1000);

        builder.HasOne<PaymentTransaction>()
            .WithMany()
            .HasForeignKey(item => item.PaymentTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => item.EventId)
            .IsUnique()
            .HasDatabaseName("UX_PaymentWebhookEvents_EventId");
    }
}
