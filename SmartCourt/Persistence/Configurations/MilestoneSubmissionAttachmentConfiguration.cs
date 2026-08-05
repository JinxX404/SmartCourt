using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Milestones.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class MilestoneSubmissionAttachmentConfiguration
    : IEntityTypeConfiguration<MilestoneSubmissionAttachment>
{
    public void Configure(EntityTypeBuilder<MilestoneSubmissionAttachment> builder)
    {
        builder.ToTable("MilestoneSubmissionAttachments");
        builder.HasKey(attachment => attachment.Id);
        builder.Property(attachment => attachment.CreatedAt).Utc();

        builder.HasOne<MilestoneSubmission>()
            .WithMany()
            .HasForeignKey(attachment => attachment.MilestoneSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<SmartCourt.Entities.StoredFile>()
            .WithMany()
            .HasForeignKey(attachment => attachment.StoredFileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
