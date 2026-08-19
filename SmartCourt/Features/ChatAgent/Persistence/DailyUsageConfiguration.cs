using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.ChatAgent.Entities;

namespace SmartCourt.Features.ChatAgent.Persistence;

internal sealed class DailyUsageConfiguration : IEntityTypeConfiguration<DailyUsage>
{
    public void Configure(EntityTypeBuilder<DailyUsage> builder)
    {
        builder.HasKey(x => new { x.ClientId, x.UsageDate });
        builder.Property(x => x.ConsumedTokens).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
