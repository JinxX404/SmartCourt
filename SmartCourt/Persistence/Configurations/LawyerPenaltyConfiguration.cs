using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Disputes.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class LawyerPenaltyConfiguration
    : IEntityTypeConfiguration<LawyerPenalty>
{
    public void Configure(EntityTypeBuilder<LawyerPenalty> builder)
    {
        builder.ToTable("LawyerPenalties");
        builder.HasKey(penalty => penalty.Id);

        builder.Property(penalty => penalty.PenaltyType)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(penalty => penalty.Reason)
            .IsRequired()
            .Unicode(2_000);
        builder.Property(penalty => penalty.StartsAt).Utc();
        builder.Property(penalty => penalty.EndsAt).NullableUtc();
        builder.Property(penalty => penalty.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false);
        builder.Property(penalty => penalty.RevokedAt).NullableUtc();
        builder.Property(penalty => penalty.RevocationReason)
            .NullableUnicode(2_000);
        builder.Property(penalty => penalty.CreatedAt).Utc();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(penalty => penalty.LawyerUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Dispute>()
            .WithMany()
            .HasForeignKey(penalty => penalty.DisputeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(penalty => penalty.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(penalty => penalty.RevokedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(penalty => new
        {
            penalty.LawyerUserId,
            penalty.StartsAt
        })
        .HasDatabaseName("IX_LawyerPenalties_LawyerUserId_StartsAt");
        builder.HasCheckConstraint(
            "CK_LawyerPenalties_Type_Range",
            "[PenaltyType] BETWEEN 0 AND 3");
        builder.HasCheckConstraint(
            "CK_LawyerPenalties_EndAfterStart",
            "[EndsAt] IS NULL OR [EndsAt] >= [StartsAt]");
    }
}
