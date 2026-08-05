using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Disputes.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class DisputeEvidenceConfiguration
    : IEntityTypeConfiguration<DisputeEvidence>
{
    public void Configure(EntityTypeBuilder<DisputeEvidence> builder)
    {
        builder.ToTable("DisputeEvidence");
        builder.HasKey(evidence => evidence.Id);

        builder.Property(evidence => evidence.Content)
            .NullableUnicode(20_000);
        builder.Property(evidence => evidence.CreatedAt).Utc();

        builder.HasOne<Dispute>()
            .WithMany()
            .HasForeignKey(evidence => evidence.DisputeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(evidence => evidence.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SmartCourt.Entities.StoredFile>()
            .WithMany()
            .HasForeignKey(evidence => evidence.StoredFileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasCheckConstraint(
            "CK_DisputeEvidence_FileOrContent",
            "[StoredFileId] IS NOT NULL OR [Content] IS NOT NULL");
    }
}
