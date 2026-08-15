using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;

namespace SmartCourt.Persistence.Configurations;

public class LegalArticleCategoryConfiguration : IEntityTypeConfiguration<LegalArticleCategory>
{
    public void Configure(EntityTypeBuilder<LegalArticleCategory> builder)
    {
        builder.ToTable("LegalArticleCategories");
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(c => c.NameAr)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.HasIndex(c => c.Code).IsUnique();

        // Seed initial categories
        builder.HasData(
            new LegalArticleCategory { Id = Guid.Parse("d3b711e7-f1e1-450a-9d9f-3d12c5b96901"), Code = "commercial", NameAr = "القانون التجاري", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("c2b711e7-f1e1-450a-9d9f-3d12c5b96902"), Code = "civil", NameAr = "القانون المدني", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("b1b711e7-f1e1-450a-9d9f-3d12c5b96903"), Code = "labor", NameAr = "نظام العمل", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("a0b711e7-f1e1-450a-9d9f-3d12c5b96904"), Code = "criminal", NameAr = "القانون الجنائي", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) }
        );
    }
}
