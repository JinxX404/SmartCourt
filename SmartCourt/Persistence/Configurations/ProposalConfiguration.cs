using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.Cases.Entities;
using SmartCourt.Features.Proposals.Entities;

namespace SmartCourt.Persistence.Configurations;

public sealed class ProposalConfiguration
    : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.ToTable("Proposals");
        builder.HasKey(proposal => proposal.Id);

        builder.Property(proposal => proposal.Status)
            .IsRequired()
            .HasConversion<int>();
        builder.Property(proposal => proposal.CreatedAt).Utc();
        builder.Property(proposal => proposal.UpdatedAt).Utc();

        builder.HasOne<LegalCase>()
            .WithMany()
            .HasForeignKey(proposal => proposal.LegalCaseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(proposal => proposal.ClientUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(proposal => proposal.LawyerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(proposal => new
        {
            proposal.LegalCaseId,
            proposal.Status
        });
        builder.HasIndex(proposal => new
        {
            proposal.ClientUserId,
            proposal.LawyerUserId,
            proposal.Status
        });
        builder.HasCheckConstraint(
            "CK_Proposals_Status_Range",
            "[Status] BETWEEN 0 AND 2");
    }
}
