using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Common.Entities;
using SmartCourt.Features.Cases.Entities;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Proposals.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");
        builder.HasKey(contract => contract.Id);

        builder.Property(contract => contract.Title)
            .IsRequired()
            .Unicode(200);
        builder.Property(contract => contract.TermsAndConditions)
            .IsRequired()
            .Unicode(20_000);
        builder.Property(contract => contract.Currency)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(3)
            .HasDefaultValue("EGP");
        builder.Property(contract => contract.Status)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(contract => contract.TerminationReason)
            .NullableUnicode(2_000);
        builder.Property(contract => contract.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
        builder.Property(contract => contract.CreatedAt).Utc();
        builder.Property(contract => contract.UpdatedAt).Utc();
        builder.Property(contract => contract.AcceptedByClientAt).NullableUtc();
        builder.Property(contract => contract.AcceptedByLawyerAt).NullableUtc();
        builder.Property(contract => contract.ActivatedAt).NullableUtc();
        builder.Property(contract => contract.CompletedAt).NullableUtc();
        builder.Property(contract => contract.TerminatedAt).NullableUtc();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(contract => contract.ClientUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(contract => contract.LawyerUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(contract => contract.TerminatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Proposal>()
            .WithOne()
            .HasForeignKey<Contract>(contract => contract.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<LegalCase>()
            .WithMany()
            .HasForeignKey(contract => contract.LegalCaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(contract => contract.ProposalId)
            .IsUnique()
            .HasDatabaseName("UX_Contracts_ProposalId");
        builder.HasIndex(contract => contract.Status)
            .HasDatabaseName("IX_Contracts_Status");

        builder.HasCheckConstraint(
            "CK_Contracts_Currency_EGP",
            "[Currency] = 'EGP'");
        builder.HasCheckConstraint(
            "CK_Contracts_Status_Range",
            "[Status] BETWEEN 0 AND 4");
    }
}
