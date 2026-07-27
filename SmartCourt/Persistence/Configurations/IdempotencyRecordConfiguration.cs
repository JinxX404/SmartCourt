using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Infrastructure.Persistence.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class IdempotencyRecordConfiguration
    : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecords");
        builder.HasKey(record => record.Id);

        builder.Property(record => record.Key)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(200);
        builder.Property(record => record.Operation)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(200);
        builder.Property(record => record.ResourceType)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(100);
        builder.Property(record => record.RequestHash)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(128);
        builder.Property(record => record.ResponseBody)
            .NullableUnicode(20_000);
        builder.Property(record => record.Status)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(record => record.ExpiresAt).Utc();
        builder.Property(record => record.CompletedAt).NullableUtc();
        builder.Property(record => record.CreatedAt).Utc();
        builder.Property(record => record.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(record => record.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(record => new
        {
            record.UserId,
            record.Key
        })
        .IsUnique()
        .HasDatabaseName("UX_IdempotencyRecords_UserId_Key");
        builder.HasIndex(record => new
        {
            record.Status,
            record.ExpiresAt
        })
        .HasDatabaseName("IX_IdempotencyRecords_Status_ExpiresAt");
        builder.HasCheckConstraint(
            "CK_IdempotencyRecords_Status_Range",
            "[Status] BETWEEN 0 AND 2");
    }
}
