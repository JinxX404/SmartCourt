using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Common.Entities;
using SmartCourt.Features.Chat.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class ChatMessageConfiguration
    : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.Type)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(message => message.Content)
            .IsRequired()
            .Unicode(ChatMessage.MaximumContentLength);
        builder.Property(message => message.SystemCode)
            .NullableUnicode(100);
        builder.Property(message => message.CreatedAt).Utc();

        builder.HasOne(message => message.Conversation)
            .WithMany(conversation => conversation.Messages)
            .HasForeignKey(message => message.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(message => message.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(message => new
            {
                message.ConversationId,
                message.CreatedAt
            })
            .HasDatabaseName("IX_ChatMessages_Conversation_CreatedAt");
        builder.HasCheckConstraint(
            "CK_ChatMessages_Type_Range",
            "[Type] BETWEEN 1 AND 2");
        builder.HasCheckConstraint(
            "CK_ChatMessages_UserOrSystem",
            "([Type] = 1 AND [SenderUserId] IS NOT NULL AND [SystemCode] IS NULL) OR ([Type] = 2 AND [SenderUserId] IS NULL AND [SystemCode] IS NOT NULL)");
    }
}
