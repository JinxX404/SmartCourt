using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Chat.Entities;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Proposals.GetProposals;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Proposals;

public sealed class ProposalListingIntegrationTests
{
    private readonly DateTime _utcNow =
        new(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc);
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _otherClientUserId = Guid.NewGuid();
    private readonly Guid _lawyerUserId = Guid.NewGuid();
    private readonly Guid _otherLawyerUserId = Guid.NewGuid();
    private readonly Guid _caseId = Guid.NewGuid();
    private readonly Guid _otherCaseId = Guid.NewGuid();

    [Fact]
    public async Task QueryDefaultsToFiveAndRejectsPageSizesAboveFifty()
    {
        var defaultQuery = new GetProposalsQuery(
            ProposalListScope.LawyerInbox);
        Assert.Equal(5, defaultQuery.PageSize);

        var validation = await new GetProposalsQueryValidator().ValidateAsync(
            defaultQuery with { PageSize = 51 });

        Assert.False(validation.IsValid);
        Assert.Contains(
            validation.Errors,
            error => error.PropertyName == nameof(GetProposalsQuery.PageSize));
    }

    [Fact]
    public async Task LawyerInbox_DefaultsToPendingAndOwnProposals()
    {
        await using var context = CreateContext();
        await SeedUsersAndCasesAsync(context);
        var expected = CreateProposal(
            _caseId,
            _lawyerUserId,
            _utcNow.AddHours(-2));
        var accepted = CreateProposal(
            _otherCaseId,
            _lawyerUserId,
            _utcNow.AddHours(-3));
        accepted.Accept(_utcNow.AddHours(-1));
        var anotherLawyersProposal = CreateProposal(
            _caseId,
            _otherLawyerUserId,
            _utcNow.AddHours(-1));
        context.AddRange(expected, accepted, anotherLawyersProposal);
        await context.SaveChangesAsync();

        var result = await CreateHandler(context, _lawyerUserId).Handle(
            new GetProposalsQuery(ProposalListScope.LawyerInbox),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(5, result.Data!.PageSize);
        var item = Assert.Single(result.Data.Items);
        Assert.Equal(expected.Id, item.Id);
        Assert.Equal(["Accept", "Reject"], item.PermittedActions);
    }

    [Fact]
    public async Task ClientCase_MultipleStatusesReturnOnlyThatCaseWithWorkflowMetadata()
    {
        await using var context = CreateContext();
        await SeedUsersAndCasesAsync(context);
        var legalCase = await context.Cases.SingleAsync(item => item.Id == _caseId);
        legalCase.Status = CaseStatus.Assigned;
        legalCase.LawyerId = _lawyerUserId;

        var selected = CreateProposal(
            _caseId,
            _lawyerUserId,
            _utcNow.AddHours(-4));
        selected.Accept(_utcNow.AddHours(-3));
        var selectedConversation = new ChatConversation(
            Guid.NewGuid(),
            selected.Id,
            _caseId,
            _clientUserId,
            _lawyerUserId,
            _utcNow.AddHours(-3));
        var contract = new Contract(
            Guid.NewGuid(),
            selected.Id,
            _caseId,
            _clientUserId,
            _lawyerUserId,
            "Representation contract",
            "Agreed legal representation terms.",
            _utcNow.AddHours(-2));
        contract.Status = ContractStatus.Active;
        contract.ActivatedAt = _utcNow.AddHours(-1);

        var superseded = CreateProposal(
            _caseId,
            _otherLawyerUserId,
            _utcNow.AddHours(-5));
        superseded.Accept(_utcNow.AddHours(-4));
        superseded.Supersede(_utcNow.AddHours(-1));
        var closedConversation = new ChatConversation(
            Guid.NewGuid(),
            superseded.Id,
            _caseId,
            _clientUserId,
            _otherLawyerUserId,
            _utcNow.AddHours(-4));
        closedConversation.Close(_utcNow.AddHours(-1));

        var anotherCaseProposal = CreateProposal(
            _otherCaseId,
            _lawyerUserId,
            _utcNow.AddMinutes(-30));
        context.AddRange(
            selected,
            selectedConversation,
            contract,
            superseded,
            closedConversation,
            anotherCaseProposal);
        await context.SaveChangesAsync();

        var result = await CreateHandler(context, _clientUserId).Handle(
            new GetProposalsQuery(
                ProposalListScope.ClientCase,
                _caseId,
                [ProposalStatus.Accepted, ProposalStatus.Superseded]),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.TotalCount);
        var selectedItem = Assert.Single(
            result.Data.Items,
            item => item.Id == selected.Id);
        Assert.Equal("Assigned", selectedItem.CaseStatus);
        Assert.Equal(_lawyerUserId, selectedItem.AssignedLawyerUserId);
        Assert.True(selectedItem.IsAssignedLawyer);
        Assert.Equal(contract.Id, selectedItem.ContractId);
        Assert.Equal("Active", selectedItem.ContractStatus);
        Assert.Equal("Open", selectedItem.ConversationStatus);
        Assert.True(selectedItem.CanChat);
        Assert.Equal(
            ["OpenChat", "ViewContract"],
            selectedItem.PermittedActions);

        var supersededItem = Assert.Single(
            result.Data.Items,
            item => item.Id == superseded.Id);
        Assert.False(supersededItem.IsAssignedLawyer);
        Assert.Equal("Closed", supersededItem.ConversationStatus);
        Assert.False(supersededItem.CanChat);
        Assert.Equal(
            ["ViewChatHistory"],
            supersededItem.PermittedActions);
    }

    [Fact]
    public async Task ClientCase_DoesNotExposeAnotherClientsCase()
    {
        await using var context = CreateContext();
        await SeedUsersAndCasesAsync(context);

        var result = await CreateHandler(context, _otherClientUserId).Handle(
            new GetProposalsQuery(
                ProposalListScope.ClientCase,
                _caseId),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task LawyerInbox_HidesSupersededConversationMetadataAndActions()
    {
        await using var context = CreateContext();
        await SeedUsersAndCasesAsync(context);
        var proposal = CreateProposal(
            _caseId,
            _lawyerUserId,
            _utcNow.AddHours(-3));
        proposal.Accept(_utcNow.AddHours(-2));
        proposal.Supersede(_utcNow.AddHours(-1));
        var conversation = new ChatConversation(
            Guid.NewGuid(),
            proposal.Id,
            _caseId,
            _clientUserId,
            _lawyerUserId,
            _utcNow.AddHours(-2));
        conversation.Close(_utcNow.AddHours(-1));
        context.AddRange(proposal, conversation);
        await context.SaveChangesAsync();

        var result = await CreateHandler(context, _lawyerUserId).Handle(
            new GetProposalsQuery(
                ProposalListScope.LawyerInbox,
                Statuses: [ProposalStatus.Superseded]),
            CancellationToken.None);

        Assert.True(result.Success);
        var item = Assert.Single(result.Data!.Items);
        Assert.Null(item.ConversationId);
        Assert.Null(item.ConversationStatus);
        Assert.False(item.CanChat);
        Assert.DoesNotContain("OpenChat", item.PermittedActions);
        Assert.DoesNotContain("ViewChatHistory", item.PermittedActions);
    }

    [Theory]
    [InlineData(ContractStatus.Completed)]
    [InlineData(ContractStatus.Terminated)]
    public async Task LawyerInbox_HidesTerminalContractConversationMetadataAndActions(
        ContractStatus terminalStatus)
    {
        await using var context = CreateContext();
        await SeedUsersAndCasesAsync(context);
        var proposal = CreateProposal(
            _caseId,
            _lawyerUserId,
            _utcNow.AddHours(-3));
        proposal.Accept(_utcNow.AddHours(-2));
        var conversation = new ChatConversation(
            Guid.NewGuid(),
            proposal.Id,
            _caseId,
            _clientUserId,
            _lawyerUserId,
            _utcNow.AddHours(-2));
        conversation.Close(_utcNow.AddHours(-1));
        var contract = new Contract(
            Guid.NewGuid(),
            proposal.Id,
            _caseId,
            _clientUserId,
            _lawyerUserId,
            "Representation contract",
            "Agreed terms.",
            _utcNow.AddHours(-2));
        contract.Status = terminalStatus;
        context.AddRange(proposal, conversation, contract);
        await context.SaveChangesAsync();

        var result = await CreateHandler(context, _lawyerUserId).Handle(
            new GetProposalsQuery(
                ProposalListScope.LawyerInbox,
                Statuses: [ProposalStatus.Accepted]),
            CancellationToken.None);

        Assert.True(result.Success);
        var item = Assert.Single(result.Data!.Items);
        Assert.Null(item.ConversationId);
        Assert.Null(item.ConversationStatus);
        Assert.False(item.CanChat);
        Assert.DoesNotContain("OpenChat", item.PermittedActions);
        Assert.DoesNotContain("ViewChatHistory", item.PermittedActions);
    }

    private GetProposalsHandler CreateHandler(
        ApplicationDbContext context,
        Guid userId)
    {
        return new GetProposalsHandler(
            context,
            new TestCurrentUserService(userId),
            new GetProposalsQueryValidator());
    }

    private Proposal CreateProposal(
        Guid legalCaseId,
        Guid lawyerUserId,
        DateTime createdAt)
    {
        return new Proposal(
            Guid.NewGuid(),
            legalCaseId,
            _clientUserId,
            lawyerUserId,
            "Please review this case.",
            createdAt);
    }

    private async Task SeedUsersAndCasesAsync(ApplicationDbContext context)
    {
        var clientRole = new IdentityRole<Guid>("Client")
        {
            Id = Guid.NewGuid(),
            NormalizedName = "CLIENT"
        };
        var lawyerRole = new IdentityRole<Guid>("Lawyer")
        {
            Id = Guid.NewGuid(),
            NormalizedName = "LAWYER"
        };
        context.Roles.AddRange(clientRole, lawyerRole);
        context.Users.AddRange(
            CreateUser(_clientUserId, "client"),
            CreateUser(_otherClientUserId, "other-client"),
            CreateUser(_lawyerUserId, "lawyer"),
            CreateUser(_otherLawyerUserId, "other-lawyer"));
        context.UserRoles.AddRange(
            new IdentityUserRole<Guid>
            {
                UserId = _clientUserId,
                RoleId = clientRole.Id
            },
            new IdentityUserRole<Guid>
            {
                UserId = _otherClientUserId,
                RoleId = clientRole.Id
            },
            new IdentityUserRole<Guid>
            {
                UserId = _lawyerUserId,
                RoleId = lawyerRole.Id
            },
            new IdentityUserRole<Guid>
            {
                UserId = _otherLawyerUserId,
                RoleId = lawyerRole.Id
            });
        context.Cases.AddRange(
            new SmartCourt.Entities.Case
            {
                Id = _caseId,
                ClientId = _clientUserId,
                Title = "Employment dispute",
                Description = "Employment dispute details.",
                Status = CaseStatus.Matched,
                SubmittedAt = _utcNow.AddDays(-1)
            },
            new SmartCourt.Entities.Case
            {
                Id = _otherCaseId,
                ClientId = _clientUserId,
                Title = "Lease dispute",
                Description = "Lease dispute details.",
                Status = CaseStatus.Matched,
                SubmittedAt = _utcNow.AddDays(-1)
            });
        await context.SaveChangesAsync();
    }

    private static ApplicationUser CreateUser(Guid id, string userName)
    {
        return new ApplicationUser
        {
            Id = id,
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = $"{userName}@example.test",
            NormalizedEmail = $"{userName}@example.test".ToUpperInvariant(),
            FullName = userName,
            NationalNumber = id.ToString("N")[..14],
            Status = UserStatus.Active
        };
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"proposal-list-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(
            options,
            new FixedTimeProvider(_utcNow));
    }

    private sealed class TestCurrentUserService(Guid userId)
        : ICurrentUserService
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => true;
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
