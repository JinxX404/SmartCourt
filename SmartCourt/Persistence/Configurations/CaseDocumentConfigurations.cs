using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Entities;

namespace SmartCourt.Persistence.Configurations
{
    public class CaseDocumentConfigurations : IEntityTypeConfiguration<CaseDocument>
    {
        public void Configure(EntityTypeBuilder<CaseDocument> builder)
        {
            builder.HasOne(cd => cd.Case)
                .WithMany(c => c.Documents)
                .HasForeignKey(c => c.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cd => cd.StoredFile)
                .WithOne()
                .HasForeignKey<CaseDocument>(c => c.StoredFileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
