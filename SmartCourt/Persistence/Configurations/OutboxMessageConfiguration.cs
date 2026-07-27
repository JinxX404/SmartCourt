using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Infrastructure.Persistence.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.EventType)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(200);
        builder.Property(message => message.Payload)
            .IsRequired()
            .Unicode(20_000);
        builder.Property(message => message.AggregateType)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(100);
        builder.Property(message => message.Status)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(message => message.LastError)
            .NullableUnicode(2_000);
        builder.Property(message => message.AvailableAt).Utc();
        builder.Property(message => message.LeaseExpiresAt).NullableUtc();
        builder.Property(message => message.ProcessedAt).NullableUtc();
        builder.Property(message => message.CreatedAt).Utc();
        builder.Property(message => message.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(message => new
        {
            message.Status,
            message.AvailableAt
        })
        .HasDatabaseName("IX_OutboxMessages_Status_AvailableAt");
        builder.HasIndex(message => new
        {
            message.AggregateType,
            message.AggregateId
        })
        .HasDatabaseName("IX_OutboxMessages_Aggregate");
        builder.HasCheckConstraint(
            "CK_OutboxMessages_EventVersion_Positive",
            "[EventVersion] > 0");
        builder.HasCheckConstraint(
            "CK_OutboxMessages_Status_Range",
            "[Status] BETWEEN 0 AND 3");
        builder.HasCheckConstraint(
            "CK_OutboxMessages_Attempts_NonNegative",
            "[Attempts] >= 0");
    }
}
