using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Common.Entities;
using SmartCourt.Entities;
using SmartCourt.Features.Chat.Entities;
using SmartCourt.Features.Proposals.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class ChatConversationConfiguration
    : IEntityTypeConfiguration<ChatConversation>
{
    public void Configure(EntityTypeBuilder<ChatConversation> builder)
    {
        builder.ToTable("ChatConversations");
        builder.HasKey(conversation => conversation.Id);

        builder.Property(conversation => conversation.CreatedAt).Utc();
        builder.Property(conversation => conversation.UpdatedAt).Utc();
        builder.Property(conversation => conversation.LastMessageAt).NullableUtc();
        builder.Property(conversation => conversation.IsClosed)
            .HasDefaultValue(false);

        builder.HasOne(conversation => conversation.Proposal)
            .WithOne()
            .HasForeignKey<ChatConversation>(conversation => conversation.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(conversation => conversation.Case)
            .WithMany()
            .HasForeignKey(conversation => conversation.LegalCaseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(conversation => conversation.ClientUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(conversation => conversation.LawyerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(conversation => conversation.ProposalId)
            .IsUnique()
            .HasDatabaseName("UX_ChatConversations_ProposalId");
        builder.HasIndex(conversation => new
            {
                conversation.ClientUserId,
                conversation.UpdatedAt
            })
            .HasDatabaseName("IX_ChatConversations_Client_UpdatedAt");
        builder.HasIndex(conversation => new
            {
                conversation.LawyerUserId,
                conversation.UpdatedAt
            })
            .HasDatabaseName("IX_ChatConversations_Lawyer_UpdatedAt");
    }
}
