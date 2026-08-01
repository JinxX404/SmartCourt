using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Common.Entities;
using SmartCourt.Features.Contracts.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class ContractFileAccessAuditConfiguration
    : IEntityTypeConfiguration<ContractFileAccessAudit>
{
    public void Configure(
        EntityTypeBuilder<ContractFileAccessAudit> builder)
    {
        builder.ToTable(
            "ContractFileAccessAudits",
            table => table.HasCheckConstraint(
                "CK_ContractFileAccessAudits_Purpose_Range",
                "[Purpose] BETWEEN 1 AND 3"));
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Purpose)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(item => item.ModeratorAccess).IsRequired();
        builder.Property(item => item.AccessedAt).Utc();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(item => item.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new
            {
                item.StoredFileId,
                item.RelatedEntityId,
                item.AccessedAt
            })
            .HasDatabaseName("IX_ContractFileAccessAudits_File_Entity_Time");
    }
}
