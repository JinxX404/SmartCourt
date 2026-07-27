using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Milestones.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class MilestoneChangeRequestConfiguration
    : IEntityTypeConfiguration<MilestoneChangeRequest>
{
    public void Configure(EntityTypeBuilder<MilestoneChangeRequest> builder)
    {
        builder.ToTable("MilestoneChangeRequests");
        builder.HasKey(request => request.Id);

        builder.Property(request => request.ProposedDescription)
            .NullableUnicode(10_000);
        builder.Property(request => request.Reason)
            .IsRequired()
            .Unicode(2_000);
        builder.Property(request => request.Status)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(request => request.ProposedDueDate).NullableUtc();
        builder.Property(request => request.DecidedAt).NullableUtc();
        builder.Property(request => request.CreatedAt).Utc();
        builder.Property(request => request.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne<Milestone>()
            .WithMany()
            .HasForeignKey(request => request.MilestoneId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(request => request.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(request => request.DecidedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(request => new
        {
            request.MilestoneId,
            request.Status
        })
        .HasFilter("[Status] = 0")
        .IsUnique()
        .HasDatabaseName("UX_MilestoneChangeRequests_Pending");
        builder.HasCheckConstraint(
            "CK_MilestoneChangeRequests_DurationDays_Range",
            "[ProposedDurationDays] IS NULL OR [ProposedDurationDays] BETWEEN 1 AND 365");
        builder.HasCheckConstraint(
            "CK_MilestoneChangeRequests_Status_Range",
            "[Status] BETWEEN 0 AND 3");
    }
}
