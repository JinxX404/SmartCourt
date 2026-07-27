using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Disputes.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class DisputeResolutionConfiguration
    : IEntityTypeConfiguration<DisputeResolution>
{
    public void Configure(EntityTypeBuilder<DisputeResolution> builder)
    {
        builder.ToTable("DisputeResolutions");
        builder.HasKey(resolution => resolution.Id);

        builder.Property(resolution => resolution.ResolutionType)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(resolution => resolution.GrossHoldAmount).Money();
        builder.Property(resolution => resolution.ClientRefundAmount).Money();
        builder.Property(resolution => resolution.LawyerReleaseAmount).Money();
        builder.Property(resolution => resolution.PlatformFeeAmount).Money();
        builder.Property(resolution => resolution.Summary)
            .IsRequired()
            .Unicode(2_000);
        builder.Property(resolution => resolution.ResolvedAt).Utc();
        builder.Property(resolution => resolution.CreatedAt).Utc();

        builder.HasOne<Dispute>()
            .WithOne()
            .HasForeignKey<DisputeResolution>(
                resolution => resolution.DisputeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(resolution => resolution.ResolvedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(resolution => resolution.DisputeId)
            .IsUnique()
            .HasDatabaseName("UX_DisputeResolutions_DisputeId");
        builder.HasCheckConstraint(
            "CK_DisputeResolutions_Amounts_NonNegative",
            "[GrossHoldAmount] >= 0 AND [ClientRefundAmount] >= 0 "
            + "AND [LawyerReleaseAmount] >= 0 AND [PlatformFeeAmount] >= 0");
        builder.HasCheckConstraint(
            "CK_DisputeResolutions_Reconciliation",
            "[GrossHoldAmount] = [ClientRefundAmount] "
            + "+ [LawyerReleaseAmount] + [PlatformFeeAmount]");
        builder.HasCheckConstraint(
            "CK_DisputeResolutions_ResolutionType_Range",
            "[ResolutionType] BETWEEN 0 AND 2");
    }
}
