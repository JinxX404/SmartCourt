using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.ChatAgent.Entities;

namespace SmartCourt.Features.ChatAgent.Persistence;

internal sealed class QuotaTransactionConfiguration : IEntityTypeConfiguration<QuotaTransaction>
{
    public void Configure(EntityTypeBuilder<QuotaTransaction> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ClientId).IsRequired();
        builder.Property(x => x.Amount).IsRequired();
        builder.Property(x => x.Reason).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ReferenceId).HasMaxLength(200);
        builder.HasIndex(x => x.ClientId);
    }
}
