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
        builder.Property(item => item.ReceivedAt).Utc();

        builder.HasOne<PaymentTransaction>()
            .WithMany()
            .HasForeignKey(item => item.PaymentTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => item.EventId)
            .IsUnique()
            .HasDatabaseName("UX_PaymentWebhookEvents_EventId");
    }
}
