using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.LawyerSubscription.Entities;

namespace SmartCourt.Features.LawyerSubscription.Persistence;

internal sealed class LawyerQuotaTransactionConfiguration : IEntityTypeConfiguration<LawyerQuotaTransaction>
{
    public void Configure(EntityTypeBuilder<LawyerQuotaTransaction> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LawyerId).IsRequired();
        builder.Property(x => x.Amount).IsRequired();
        builder.Property(x => x.Reason).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ReferenceId).HasMaxLength(200);
        builder.HasIndex(x => x.LawyerId);
    }
}
