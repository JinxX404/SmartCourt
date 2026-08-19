using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawyerSubscriptionEntity = SmartCourt.Features.LawyerSubscription.Entities.LawyerSubscription;

namespace SmartCourt.Features.LawyerSubscription.Persistence;

internal sealed class LawyerSubscriptionConfiguration : IEntityTypeConfiguration<LawyerSubscriptionEntity>
{
    public void Configure(EntityTypeBuilder<LawyerSubscriptionEntity> builder)
    {
        builder.HasKey(x => x.LawyerId);
        builder.Property(x => x.PlanType).IsRequired();
        builder.Property(x => x.DailyTokenLimit).IsRequired();
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
