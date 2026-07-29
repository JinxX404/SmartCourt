using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Cases.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class LegalCaseConfiguration
    : IEntityTypeConfiguration<LegalCase>
{
    public void Configure(EntityTypeBuilder<LegalCase> builder)
    {
        builder.ToTable("LegalCases");
        builder.HasKey(legalCase => legalCase.Id);

        builder.Property(legalCase => legalCase.Title)
            .IsRequired()
            .Unicode(200);
        builder.Property(legalCase => legalCase.Description)
            .IsRequired()
            .Unicode(10_000);
        builder.Property(legalCase => legalCase.CaseLocation)
            .NullableUnicode(500);
        builder.Property(legalCase => legalCase.Status)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(legalCase => legalCase.FinalSubmittedAt)
            .NullableUtc();
        builder.Property(legalCase => legalCase.CreatedAt).Utc();
        builder.Property(legalCase => legalCase.UpdatedAt).Utc();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(legalCase => legalCase.ClientUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(legalCase => new
        {
            legalCase.ClientUserId,
            legalCase.Status
        });
        builder.HasCheckConstraint(
            "CK_LegalCases_Status_Range",
            "[Status] BETWEEN 0 AND 4");
    }
}
