using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Notifications.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class NotificationConfiguration
    : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(notification => notification.Id);

        builder.Property(notification => notification.Sequence)
            .UseIdentityColumn();
        builder.Property(notification => notification.Type)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(Notification.MaximumTypeLength);
        builder.Property(notification => notification.Severity)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(notification => notification.Title)
            .IsRequired()
            .Unicode(Notification.MaximumTitleLength);
        builder.Property(notification => notification.Body)
            .IsRequired()
            .Unicode(Notification.MaximumBodyLength);
        builder.Property(notification => notification.ActionUrl)
            .NullableUnicode(Notification.MaximumActionUrlLength);
        builder.Property(notification => notification.DataJson)
            .NullableUnicode(Notification.MaximumDataJsonLength);
        builder.Property(notification => notification.CreatedAtUtc).Utc();
        builder.Property(notification => notification.ReadAtUtc).NullableUtc();
        builder.Property(notification => notification.ExpiresAtUtc).NullableUtc();
        builder.Property(notification => notification.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
        builder.Ignore(notification => notification.IsRead);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(notification => notification.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(notification => notification.Sequence)
            .IsUnique()
            .HasDatabaseName("UX_Notifications_Sequence");
        builder.HasIndex(notification => new
            {
                notification.SourceEventId,
                notification.RecipientUserId,
                notification.Type
            })
            .IsUnique()
            .HasDatabaseName("UX_Notifications_Source_Recipient_Type");
        builder.HasIndex(notification => new
            {
                notification.RecipientUserId,
                notification.Sequence
            })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Notifications_Recipient_Sequence");
        builder.HasIndex(notification => new
            {
                notification.RecipientUserId,
                notification.ReadAtUtc,
                notification.Sequence
            })
            .IsDescending(false, false, true)
            .HasFilter("[ReadAtUtc] IS NULL")
            .HasDatabaseName("IX_Notifications_Recipient_Unread_Sequence");
        builder.HasCheckConstraint(
            "CK_Notifications_Severity_Range",
            "[Severity] BETWEEN 1 AND 4");
    }
}
