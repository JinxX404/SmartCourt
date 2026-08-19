using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.ChatAgent.Entities;

namespace SmartCourt.Features.ChatAgent.Persistence;

internal sealed class QuotaProfileConfiguration : IEntityTypeConfiguration<QuotaProfile>
{
    public void Configure(EntityTypeBuilder<QuotaProfile> builder)
    {
        builder.HasKey(x => x.ClientId);
        builder.Property(x => x.DailyTokenLimit).IsRequired();
    }
}
