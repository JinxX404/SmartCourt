using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Contracts.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class ContractStateHistoryConfiguration
    : IEntityTypeConfiguration<ContractStateHistory>
{
    public void Configure(EntityTypeBuilder<ContractStateHistory> builder)
    {
        builder.ToTable("ContractStateHistories");
        builder.HasKey(history => history.Id);
        builder.Property(history => history.PreviousStatus)
            .HasConversion<int>();
        builder.Property(history => history.NewStatus)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(history => history.Trigger)
            .IsRequired()
            .Unicode(100);
        builder.Property(history => history.Reason)
            .NullableUnicode(2_000);
        builder.Property(history => history.CreatedAt).Utc();

        builder.HasOne<Contract>()
            .WithMany()
            .HasForeignKey(history => history.ContractId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(history => history.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(history => new
        {
            history.ContractId,
            history.CreatedAt
        })
        .HasDatabaseName("IX_ContractStateHistories_ContractId_CreatedAt");
        builder.HasCheckConstraint(
            "CK_ContractStateHistories_NewStatus_Range",
            "[NewStatus] BETWEEN 0 AND 4");
        builder.HasCheckConstraint(
            "CK_ContractStateHistories_PreviousStatus_Range",
            "[PreviousStatus] IS NULL OR [PreviousStatus] BETWEEN 0 AND 4");
    }
}
