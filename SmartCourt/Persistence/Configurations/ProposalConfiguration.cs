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
        builder.Property(proposal => proposal.Message)
            .IsRequired()
            .Unicode(2_000);
        builder.Property(proposal => proposal.DecisionReason)
            .NullableUnicode(1_000);
        builder.Property(proposal => proposal.RespondedAt).NullableUtc();
        builder.Property(proposal => proposal.CreatedAt).Utc();
        builder.Property(proposal => proposal.UpdatedAt).Utc();

        builder.HasOne(proposal => proposal.LegalCase)
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
            proposal.LegalCaseId,
            proposal.LawyerUserId
        }).HasFilter("[Status] IN (0, 1)").IsUnique();
        builder.HasIndex(proposal => proposal.LegalCaseId)
            .HasFilter("[Status] = 1")
            .IsUnique();
        builder.HasCheckConstraint(
            "CK_Proposals_Status_Range",
            "[Status] BETWEEN 0 AND 2");
    }
}
