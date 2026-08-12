using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Chat.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class ChatMessageAttachmentConfiguration
    : IEntityTypeConfiguration<ChatMessageAttachment>
{
    public void Configure(EntityTypeBuilder<ChatMessageAttachment> builder)
    {
        builder.ToTable("ChatMessageAttachments");
        builder.HasKey(attachment => attachment.Id);
        builder.Property(attachment => attachment.CreatedAt).Utc();

        builder.HasOne(attachment => attachment.Message)
            .WithMany(message => message.Attachments)
            .HasForeignKey(attachment => attachment.MessageId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(attachment => attachment.StoredFile)
            .WithMany()
            .HasForeignKey(attachment => attachment.StoredFileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(attachment => attachment.MessageId)
            .HasDatabaseName("IX_ChatMessageAttachments_MessageId");
        builder.HasIndex(attachment => attachment.StoredFileId)
            .IsUnique()
            .HasDatabaseName("UX_ChatMessageAttachments_StoredFileId");
    }
}
