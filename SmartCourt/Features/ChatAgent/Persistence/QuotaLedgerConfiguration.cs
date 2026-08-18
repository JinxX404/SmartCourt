using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.ChatAgent.Entities;

namespace SmartCourt.Features.ChatAgent.Persistence;

internal sealed class QuotaLedgerConfiguration : IEntityTypeConfiguration<QuotaLedger>
{
    public void Configure(EntityTypeBuilder<QuotaLedger> builder)
    {
        builder.HasKey(x => x.ClientId);
        builder.Property(x => x.AdditionalTokenBalance).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
