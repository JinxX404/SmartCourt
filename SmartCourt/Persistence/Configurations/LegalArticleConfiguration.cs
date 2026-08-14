using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Common.Entities;

namespace SmartCourt.Persistence.Configurations;

public class LegalArticleConfiguration : IEntityTypeConfiguration<LegalArticle>
{
    public void Configure(EntityTypeBuilder<LegalArticle> builder)
    {
        builder.ToTable("LegalArticles");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.Content)
            .IsRequired();

        builder.Property(a => a.Tags)
            .HasMaxLength(500);

        builder.Property(a => a.FeaturedImageUrl)
            .HasMaxLength(1000);

        builder.HasOne(a => a.Category)
            .WithMany(c => c.Articles)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Author)
            .WithMany()
            .HasForeignKey(a => a.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
