using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.LawyerSubscription.Entities;

namespace SmartCourt.Features.LawyerSubscription.Persistence;

internal sealed class LawyerDailyUsageConfiguration : IEntityTypeConfiguration<LawyerDailyUsage>
{
    public void Configure(EntityTypeBuilder<LawyerDailyUsage> builder)
    {
        builder.HasKey(x => new { x.LawyerId, x.UsageDate });
        builder.Property(x => x.ConsumedTokens).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
