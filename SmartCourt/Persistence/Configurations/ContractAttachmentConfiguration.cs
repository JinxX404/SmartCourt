using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Contracts.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class ContractAttachmentConfiguration
    : IEntityTypeConfiguration<ContractAttachment>
{
    public void Configure(EntityTypeBuilder<ContractAttachment> builder)
    {
        builder.ToTable("ContractAttachments");
        builder.HasKey(attachment => attachment.Id);
        builder.Property(attachment => attachment.CreatedAt).Utc();

        builder.HasOne<SmartCourt.Features.Contracts.Entities.Contract>()
            .WithMany()
            .HasForeignKey(attachment => attachment.ContractId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SmartCourt.Entities.StoredFile>()
            .WithMany()
            .HasForeignKey(attachment => attachment.StoredFileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(attachment => attachment.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
