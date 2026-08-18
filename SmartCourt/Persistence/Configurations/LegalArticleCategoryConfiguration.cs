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

        builder.HasData(
            new LegalArticleCategory { Id = Guid.Parse("d3b711e7-f1e1-450a-9d9f-3d12c5b96901"), Code = "commercial", NameAr = "القانون التجاري", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("c2b711e7-f1e1-450a-9d9f-3d12c5b96902"), Code = "civil", NameAr = "القانون المدني", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("b1b711e7-f1e1-450a-9d9f-3d12c5b96903"), Code = "labor", NameAr = "نظام العمل والعمال", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("a0b711e7-f1e1-450a-9d9f-3d12c5b96904"), Code = "criminal", NameAr = "القانون الجنائي", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("a1b711e7-f1e1-450a-9d9f-3d12c5b96905"), Code = "family", NameAr = "قانون الأسرة والأحوال الشخصية", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("a2b711e7-f1e1-450a-9d9f-3d12c5b96906"), Code = "administrative", NameAr = "القضاء الإداري ومجلس الدولة", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("a3b711e7-f1e1-450a-9d9f-3d12c5b96907"), Code = "constitutional", NameAr = "القانون الدستوري", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("a4b711e7-f1e1-450a-9d9f-3d12c5b96908"), Code = "tax", NameAr = "قانون الضرائب", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("a5b711e7-f1e1-450a-9d9f-3d12c5b96909"), Code = "customs", NameAr = "قانون الجمارك", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("a6b711e7-f1e1-450a-9d9f-3d12c5b96910"), Code = "corporate", NameAr = "قانون الشركات", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("a7b711e7-f1e1-450a-9d9f-3d12c5b96911"), Code = "contracts", NameAr = "صياغة ومنازعات العقود", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("a8b711e7-f1e1-450a-9d9f-3d12c5b96912"), Code = "intellectual-property", NameAr = "الملكية الفكرية وبراءات الاختراع", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("a9b711e7-f1e1-450a-9d9f-3d12c5b96913"), Code = "arbitration", NameAr = "التحكيم وفض المنازعات", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("b0b711e7-f1e1-450a-9d9f-3d12c5b96914"), Code = "banking-finance", NameAr = "البنوك والتمويل والأسواق المالية", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("b2b711e7-f1e1-450a-9d9f-3d12c5b96915"), Code = "investment", NameAr = "قانون الاستثمار والمناطق الحرة", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("b3b711e7-f1e1-450a-9d9f-3d12c5b96916"), Code = "real-estate", NameAr = "العقارات والشهر العقاري", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("b4b711e7-f1e1-450a-9d9f-3d12c5b96917"), Code = "execution", NameAr = "منازعات وإجراءات التنفيذ", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("b5b711e7-f1e1-450a-9d9f-3d12c5b96918"), Code = "insurance", NameAr = "قانون التأمين والتعويضات", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("b6b711e7-f1e1-450a-9d9f-3d12c5b96919"), Code = "environment", NameAr = "قانون البيئة وحمايتها", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("b7b711e7-f1e1-450a-9d9f-3d12c5b96920"), Code = "it-telecom", NameAr = "الاتصالات وتكنولوجيا المعلومات", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new LegalArticleCategory { Id = Guid.Parse("b8b711e7-f1e1-450a-9d9f-3d12c5b96921"), Code = "cybercrimes", NameAr = "الجرائم الإلكترونية والأمن السيبراني", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) }
        );
    }
}
