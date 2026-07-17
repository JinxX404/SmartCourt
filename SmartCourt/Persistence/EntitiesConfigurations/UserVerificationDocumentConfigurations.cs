using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Entities;

namespace SmartCourt.Persistence.EntitiesConfigurations
{
    public class UserVerificationDocumentConfigurations : IEntityTypeConfiguration<UserVerificationDocument>
    {
        public void Configure(EntityTypeBuilder<UserVerificationDocument> builder)
        {
            builder.HasOne(vd => vd.StoredFile)
                .WithOne()
                .HasForeignKey<UserVerificationDocument>(vd => vd.StoredFileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(vd => vd.User)
                .WithMany(u => u.VerificationDocuments)
                .HasForeignKey(vd => vd.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
