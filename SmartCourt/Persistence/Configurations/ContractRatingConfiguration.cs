using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Common.Entities;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Ratings.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class ContractRatingConfiguration : IEntityTypeConfiguration<ContractRating>
{
    public void Configure(EntityTypeBuilder<ContractRating> builder)
    {
        builder.ToTable("ContractRatings");
        builder.HasKey(rating => rating.Id);

        builder.Property(rating => rating.Stars)
            .IsRequired();

        builder.Property(rating => rating.RaterRole)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(rating => rating.Comment)
            .NullableUnicode(500);

        builder.Property(rating => rating.CreatedAt)
            .Utc();

        builder.HasOne<Contract>()
            .WithMany()
            .HasForeignKey(rating => rating.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(rating => rating.RaterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(rating => rating.RatedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(rating => new { rating.ContractId, rating.RaterRole })
            .IsUnique()
            .HasDatabaseName("UX_ContractRatings_Contract_RaterRole");

        builder.HasIndex(rating => rating.RatedUserId)
            .HasFilter("[RaterRole] = 0")
            .HasDatabaseName("IX_ContractRatings_RatedUser_ClientRatings");

        builder.HasCheckConstraint(
            "CK_ContractRatings_Stars_Range",
            "[Stars] BETWEEN 1 AND 5");

        builder.HasCheckConstraint(
            "CK_ContractRatings_RaterRole_Range",
            "[RaterRole] IN (0, 1)");
    }
}
