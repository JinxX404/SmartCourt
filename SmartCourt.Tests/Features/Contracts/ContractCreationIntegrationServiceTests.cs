using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Cases.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Cases.Integration;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Proposals.Integration;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Contracts;

public sealed class ContractCreationIntegrationServiceTests
{
    private readonly DateTime _utcNow =
        new(2026, 7, 29, 7, 30, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ProposalAndCaseServices_ReturnOnlyEligibleFacts()
    {
        await using var context = CreateContext();
        var clientUserId = Guid.NewGuid();
        var lawyerUserId = Guid.NewGuid();
        var legalCase = new LegalCase(
            Guid.NewGuid(),
            clientUserId,
            "قضية تجارية",
            "نزاع تجاري يحتاج إلى تمثيل قانوني.",
            "القاهرة",
            _utcNow)
        {
            Status = CaseStatus.Matched
        };
        var proposal = new Proposal(
            Guid.NewGuid(),
            legalCase.Id,
            clientUserId,
            lawyerUserId,
            _utcNow)
        {
            Status = ProposalStatus.Accepted
        };
        context.LegalCases.Add(legalCase);
        context.Proposals.Add(proposal);
        await context.SaveChangesAsync();

        var proposalFacts =
            await new ProposalContractAccessService(context)
                .FindAcceptedForContractAsync(
                    proposal.Id,
                    CancellationToken.None);
        var caseFacts = await new CaseContractAccessService(context)
            .FindEligibleForContractAsync(
                legalCase.Id,
                CancellationToken.None);

        Assert.NotNull(proposalFacts);
        Assert.Equal(legalCase.Id, proposalFacts.LegalCaseId);
        Assert.Equal(clientUserId, proposalFacts.ClientUserId);
        Assert.Equal(lawyerUserId, proposalFacts.LawyerUserId);
        Assert.NotNull(caseFacts);
        Assert.Equal(clientUserId, caseFacts.ClientUserId);
    }

    [Fact]
    public async Task ProposalAndCaseServices_RejectIneligibleStates()
    {
        await using var context = CreateContext();
        var legalCase = new LegalCase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "قضية مسودة",
            "هذه القضية لم تصل إلى المطابقة.",
            null,
            _utcNow);
        var proposal = new Proposal(
            Guid.NewGuid(),
            legalCase.Id,
            legalCase.ClientUserId,
            Guid.NewGuid(),
            _utcNow);
        context.LegalCases.Add(legalCase);
        context.Proposals.Add(proposal);
        await context.SaveChangesAsync();

        Assert.Null(
            await new ProposalContractAccessService(context)
                .FindAcceptedForContractAsync(
                    proposal.Id,
                    CancellationToken.None));
        Assert.Null(
            await new CaseContractAccessService(context)
                .FindEligibleForContractAsync(
                    legalCase.Id,
                    CancellationToken.None));
    }

    [Fact]
    public async Task UserEligibilityService_DerivesActiveStateAndExactRoles()
    {
        await using var context = CreateContext();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "lawyer@example.com",
            Email = "lawyer@example.com",
            FullName = "محامٍ مختبر",
            NationalNumber = "12345678901234",
            Status = UserStatus.Active
        };
        var role = new IdentityRole<Guid>("Lawyer")
        {
            Id = Guid.NewGuid(),
            NormalizedName = "LAWYER"
        };
        context.Users.Add(user);
        context.Roles.Add(role);
        context.UserRoles.Add(new IdentityUserRole<Guid>
        {
            UserId = user.Id,
            RoleId = role.Id
        });
        await context.SaveChangesAsync();

        var result = await new ContractUserEligibilityService(context)
            .FindEligibilityAsync(
                user.Id,
                CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsActive);
        Assert.True(result.CanActAsLawyer);
        Assert.False(result.CanActAsClient);
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(
                $"contract-creation-integrations-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(
            options,
            new FixedTimeProvider(_utcNow));
    }

    private sealed class FixedTimeProvider(DateTime utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
            => new(utcNow);
    }
}
