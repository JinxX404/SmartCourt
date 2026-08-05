using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;

namespace SmartCourt.Persistence.Configurations;

public class LawyerSpecializationConfiguration : IEntityTypeConfiguration<LawyerSpecialization>
{
    public void Configure(EntityTypeBuilder<LawyerSpecialization> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Specialization)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.YearsOfExperience)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.CasesHandled)
            .IsRequired()
            .HasDefaultValue(0);

        // Unique constraint: one entry per lawyer per specialization
        builder.HasIndex(s => new { s.LawyerProfileUserId, s.Specialization })
            .IsUnique()
            .HasDatabaseName("IX_LawyerSpecialization_LawyerId_Specialization");
    }
}
