using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.LawyerSubscription.Entities;

namespace SmartCourt.Features.LawyerSubscription.Persistence;

internal sealed class LawyerQuotaLedgerConfiguration : IEntityTypeConfiguration<LawyerQuotaLedger>
{
    public void Configure(EntityTypeBuilder<LawyerQuotaLedger> builder)
    {
        builder.HasKey(x => x.LawyerId);
        builder.Property(x => x.PurchasedTokenBalance).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
