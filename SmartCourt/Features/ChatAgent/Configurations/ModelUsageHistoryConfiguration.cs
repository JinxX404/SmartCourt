using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.ChatAgent.Entities;

namespace SmartCourt.Features.ChatAgent.Configurations;

public class ModelUsageHistoryConfiguration : IEntityTypeConfiguration<ModelUsageHistory>
{
    public void Configure(EntityTypeBuilder<ModelUsageHistory> builder)
    {
        builder.ToTable("ModelUsageHistories");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.InputCost).HasPrecision(18, 6);
        builder.Property(x => x.OutputCost).HasPrecision(18, 6);
        builder.Property(x => x.TotalCost).HasPrecision(18, 6);
    }
}
