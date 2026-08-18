using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.ChatAgent.Entities;

namespace SmartCourt.Features.ChatAgent.Persistence;

public sealed class TokenUsageHistoryConfiguration : IEntityTypeConfiguration<TokenUsageHistory>
{
    public void Configure(EntityTypeBuilder<TokenUsageHistory> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ClientId).IsRequired();
        builder.Property(x => x.ConversationId).IsRequired();
        builder.Property(x => x.Model).IsRequired().HasMaxLength(100);
        builder.Property(x => x.InputTokens).IsRequired();
        builder.Property(x => x.OutputTokens).IsRequired();
        builder.Property(x => x.TotalTokens).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        // Indexes for querying history by client or conversation
        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.ConversationId);
    }
}
