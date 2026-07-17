using SmartCourt.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.OwnsMany(u => u.RefreshTokens)
            .ToTable("RefreshTokens")
            .WithOwner()
            .HasForeignKey("UserId");

        builder.Property(u => u.UserName)
            .IsRequired();

        builder.Property(u => u.Email)
            .IsRequired();

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnType("nvarchar(150)");

        builder.Property(u => u.NationalNumber)
            .IsRequired()
            .HasMaxLength(14)
            .HasColumnType("varchar(14)");

        builder.Property(u => u.Gender)
            .HasMaxLength(20)
            .HasColumnType("varchar(20)");

        builder.Property(u => u.DateOfBirth)
            .HasColumnType("date");

        builder.Property(u => u.Address)
            .HasMaxLength(500)
            .HasColumnType("nvarchar(500)");

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(UserStatus.Unverified);


        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_ApplicationUser_Email");

        builder.HasIndex(u => u.NationalNumber)
            .IsUnique()
            .HasDatabaseName("IX_ApplicationUser_NationalNumber");

        builder.HasIndex(u => u.Status)
            .HasDatabaseName("IX_ApplicationUser_Status");

        builder.HasOne(u => u.LawyerProfile)
            .WithOne(p => p.User)
            .HasForeignKey<LawyerProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.ClientProfile)
            .WithOne(p => p.User)
            .HasForeignKey<ClientProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LawyerProfileConfiguration : IEntityTypeConfiguration<LawyerProfile>
{
    public void Configure(EntityTypeBuilder<LawyerProfile> builder)
    {
        builder.HasKey(p => p.UserId);
        
        builder.Property(p => p.Specialization)
            .HasMaxLength(150);
            
        builder.Property(p => p.Bio)
            .HasMaxLength(500);
            
        builder.Property(p => p.Address)
            .HasMaxLength(255);
    }
}

public class ClientProfileConfiguration : IEntityTypeConfiguration<ClientProfile>
{
    public void Configure(EntityTypeBuilder<ClientProfile> builder)
    {
        builder.HasKey(p => p.UserId);
    }
}