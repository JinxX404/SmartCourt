using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Common.Entities;

namespace SmartCourt.Persistence.EntitiesConfigurations;

public class LawDocumentConfiguration : IEntityTypeConfiguration<LawDocument>
{
    public void Configure(EntityTypeBuilder<LawDocument> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.DocumentTitle)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Language)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Language);
        builder.HasIndex(x => x.DocumentTitle);
    }
}
