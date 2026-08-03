using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Common.Entities;
using SmartCourt.Entities;

namespace SmartCourt.Persistence.Configurations;

public class CaseRecommendationConfiguration : IEntityTypeConfiguration<CaseRecommendation>
{
    public void Configure(EntityTypeBuilder<CaseRecommendation> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Case)
            .WithMany(c => c.Recommendations)
            .HasForeignKey(r => r.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.LawyerProfile)
            .WithMany()
            .HasForeignKey(r => r.LawyerId)
            .HasPrincipalKey(lp => lp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.TotalScore)
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        builder.Property(r => r.LocationScore)
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        builder.Property(r => r.ExperienceScore)
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        builder.Property(r => r.RatingScore)
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        builder.Property(r => r.ResponseTimeScore)
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        builder.Property(r => r.Explanation)
            .IsRequired();

        builder.Property(r => r.Rank)
            .IsRequired();

        builder.HasIndex(r => new { r.CaseId, r.Rank })
            .HasDatabaseName("IX_CaseRecommendation_CaseId_Rank");
    }
}
