using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Entities;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Case.Integration;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.Entities;
using SmartCourt.Features.Chat.Events;
using SmartCourt.Features.Chat.Realtime;
using SmartCourt.Features.Chat.SendMessage;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Features.Contracts.Integration;
using SmartCourt.Features.Proposals.CancelProposal;
using SmartCourt.Features.Proposals.CreateProposal;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Proposals.Expiration;
using SmartCourt.Features.Proposals.TerminateProposal;
using SmartCourt.Features.Proposals.UpdateProposal;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Proposals;

public sealed class ProposalLifecycleIntegrationTests
{
    private readonly DateTime _utcNow =
        new(2026, 8, 10, 12, 0, 0, DateTimeKind.Utc);
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _caseId = Guid.NewGuid();

    [Fact]
    public async Task CreateProposal_RejectsSixthActiveProposal()
    {
        await using var context = CreateContext();
        var lawyerIds = await SeedUsersAndCaseAsync(context, lawyerCount: 6);
        for (var index = 0; index < 5; index++)
        {
            context.Proposals.Add(CreateProposal(
                lawyerIds[index],
                _utcNow.AddHours(-1)));
        }
        await context.SaveChangesAsync();

        var result = await CreateHandler(context).Handle(
            new CreateProposalCommand(
                _caseId,
                lawyerIds[5],
                "Please review the case and proposed representation."),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal(5, await context.Proposals.CountAsync());
    }

    [Fact]
    public async Task CreateProposal_ExpiresOverdueProposalAndReusesItsSlot()
    {
        await using var context = CreateContext();
        var lawyerIds = await SeedUsersAndCaseAsync(context, lawyerCount: 6);
        context.Proposals.Add(CreateProposal(
            lawyerIds[0],
            _utcNow.AddDays(-4)));
        for (var index = 1; index < 5; index++)
        {
            context.Proposals.Add(CreateProposal(
                lawyerIds[index],
                _utcNow.AddHours(-1)));
        }
        await context.SaveChangesAsync();

        var result = await CreateHandler(context).Handle(
            new CreateProposalCommand(
                _caseId,
                lawyerIds[5],
                "Replacement invitation after the response deadline."),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal(
            ProposalStatus.Expired,
            (await context.Proposals.SingleAsync(
                proposal => proposal.LawyerUserId == lawyerIds[0])).Status);
        Assert.Equal(
            5,
            await context.Proposals.CountAsync(proposal =>
                proposal.Status == ProposalStatus.Pending
                || proposal.Status == ProposalStatus.Accepted));
        Assert.Contains(
            context.OutboxMessages,
            message => message.EventType
                == ContractPaymentEventTypes.ProposalExpired);
    }

    [Fact]
    public async Task CancelPendingProposal_DoesNotCreateConversation()
    {
        await using var context = CreateContext();
        var lawyerId = (await SeedUsersAndCaseAsync(context, 1)).Single();
        var proposal = CreateProposal(lawyerId, _utcNow.AddHours(-1));
        context.Proposals.Add(proposal);
        await context.SaveChangesAsync();

        var result = await new CancelProposalHandler(
            context,
            new TestCurrentUserService(_clientUserId),
            new FixedTimeProvider(_utcNow),
            CreateOutboxWriter(context),
            new CancelProposalCommandValidator()).Handle(
                new CancelProposalCommand(
                    proposal.Id,
                    "The client selected a different approach."),
                CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(ProposalStatus.Cancelled, proposal.Status);
        Assert.Null(result.Data!.ConversationId);
        Assert.Empty(context.ChatConversations);
    }

    [Fact]
    public async Task UpdateProposal_OnlyUpdatesPendingProposalOwnedByClient()
    {
        await using var context = CreateContext();
        var lawyerId = (await SeedUsersAndCaseAsync(context, 1)).Single();
        var proposal = CreateProposal(lawyerId, _utcNow.AddHours(-1));
        context.Proposals.Add(proposal);
        await context.SaveChangesAsync();

        var handler = new UpdateProposalHandler(
            context,
            new TestCurrentUserService(_clientUserId),
            new FixedTimeProvider(_utcNow),
            new UpdateProposalCommandValidator());
        var result = await handler.Handle(
            new UpdateProposalCommand(
                proposal.Id,
                "  Updated representation proposal.  "),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Updated representation proposal.", proposal.Message);
        Assert.Equal(_utcNow, proposal.UpdatedAt);

        proposal.Accept(_utcNow.AddMinutes(1));
        await context.SaveChangesAsync();
        var rejectedUpdate = await handler.Handle(
            new UpdateProposalCommand(proposal.Id, "Another update"),
            CancellationToken.None);

        Assert.False(rejectedUpdate.Success);
        Assert.Equal(409, rejectedUpdate.StatusCode);
        Assert.Equal(
            "لا يمكن تعديل العرض في حالته الحالية بعد الآن.",
            rejectedUpdate.Message);
    }

    [Fact]
    public async Task UpdateProposal_ReturnsNotFoundForAnotherClientsProposal()
    {
        await using var context = CreateContext();
        var lawyerId = (await SeedUsersAndCaseAsync(context, 1)).Single();
        var proposal = CreateProposal(lawyerId, _utcNow.AddHours(-1));
        context.Proposals.Add(proposal);
        await context.SaveChangesAsync();

        var handler = new UpdateProposalHandler(
            context,
            new TestCurrentUserService(Guid.NewGuid()),
            new FixedTimeProvider(_utcNow),
            new UpdateProposalCommandValidator());
        var result = await handler.Handle(
            new UpdateProposalCommand(proposal.Id, "Unauthorized update"),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal(
            "Please consider representing me in this case.",
            proposal.Message);
    }

    [Fact]
    public async Task TerminateProposal_ClosesChatAndBlocksFurtherMessages()
    {
        await using var context = CreateContext();
        var lawyerId = (await SeedUsersAndCaseAsync(context, 1)).Single();
        var proposal = CreateProposal(lawyerId, _utcNow.AddHours(-2));
        proposal.Accept(_utcNow.AddHours(-1));
        var conversation = new ChatConversation(
            Guid.NewGuid(),
            proposal.Id,
            _caseId,
            _clientUserId,
            lawyerId,
            _utcNow.AddHours(-1));
        context.AddRange(proposal, conversation);
        await context.SaveChangesAsync();

        var outboxWriter = CreateOutboxWriter(context);
        var result = await new TerminateProposalHandler(
            context,
            new TestCurrentUserService(_clientUserId),
            new FixedTimeProvider(_utcNow),
            outboxWriter,
            new TerminateProposalCommandValidator()).Handle(
                new TerminateProposalCommand(
                    proposal.Id,
                    "We could not agree on the contract terms."),
                CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(ProposalStatus.Terminated, proposal.Status);

        var notifier = new RecordingChatNotifier();
        var outboxMessage = await context.OutboxMessages.SingleAsync(
            message => message.EventType
                == ContractPaymentEventTypes.ProposalTerminated);
        await new ProposalConversationOutboxHandler(context, notifier)
            .HandleAsync(outboxMessage, CancellationToken.None);

        Assert.True(conversation.IsClosed);
        Assert.Contains(
            context.ChatMessages,
            message => message.SystemCode
                == ContractPaymentEventTypes.ProposalTerminated);
        Assert.Single(notifier.Messages);

        var sendResult = await new SendChatMessageHandler(
            context,
            new TestCurrentUserService(_clientUserId),
            new SendChatMessageCommandValidator(),
            notifier,
            new FixedTimeProvider(_utcNow)).Handle(
                new SendChatMessageCommand(
                    conversation.Id,
                    "This message must not be sent."),
                CancellationToken.None);
        Assert.False(sendResult.Success);
        Assert.Equal(409, sendResult.StatusCode);
    }

    [Fact]
    public async Task ContractAssignment_SupersedesCompetingProposalsAndAssignsCase()
    {
        await using var context = CreateContext();
        var lawyerIds = await SeedUsersAndCaseAsync(context, lawyerCount: 3);
        var selected = CreateProposal(lawyerIds[0], _utcNow.AddHours(-3));
        selected.Accept(_utcNow.AddHours(-2));
        var acceptedCompetitor = CreateProposal(
            lawyerIds[1],
            _utcNow.AddHours(-3));
        acceptedCompetitor.Accept(_utcNow.AddHours(-2));
        var pendingCompetitor = CreateProposal(
            lawyerIds[2],
            _utcNow.AddHours(-1));
        var selectedConversation = new ChatConversation(
            Guid.NewGuid(),
            selected.Id,
            _caseId,
            _clientUserId,
            lawyerIds[0],
            _utcNow.AddHours(-2));
        var competingConversation = new ChatConversation(
            Guid.NewGuid(),
            acceptedCompetitor.Id,
            _caseId,
            _clientUserId,
            lawyerIds[1],
            _utcNow.AddHours(-2));
        context.AddRange(
            selected,
            acceptedCompetitor,
            pendingCompetitor,
            selectedConversation,
            competingConversation);
        await context.SaveChangesAsync();

        var service = new ContractCaseAssignmentService(
            context,
            new ChatConversationService(
                context,
                new FixedTimeProvider(_utcNow)),
            CreateOutboxWriter(context));
        await service.AssignAsync(
            new ContractCaseAssignment(
                Guid.NewGuid(),
                selected.Id,
                _caseId,
                _clientUserId,
                lawyerIds[0],
                new DateTimeOffset(_utcNow)),
            CancellationToken.None);
        await context.SaveChangesAsync();

        var legalCase = await context.Cases.SingleAsync();
        Assert.Equal(CaseStatus.Assigned, legalCase.Status);
        Assert.Equal(lawyerIds[0], legalCase.LawyerId);
        Assert.Equal(selectedConversation.Id, legalCase.ChatId);
        Assert.Equal(ProposalStatus.Accepted, selected.Status);
        Assert.Equal(ProposalStatus.Superseded, acceptedCompetitor.Status);
        Assert.Equal(ProposalStatus.Superseded, pendingCompetitor.Status);

        var notifier = new RecordingChatNotifier();
        var closeMessage = await context.OutboxMessages.SingleAsync(
            message => message.AggregateId == acceptedCompetitor.Id);
        await new ProposalConversationOutboxHandler(context, notifier)
            .HandleAsync(closeMessage, CancellationToken.None);
        Assert.True(competingConversation.IsClosed);
    }

    [Fact]
    public async Task ContractAssignment_CreatesAndLinksMissingWinningConversation()
    {
        await using var context = CreateContext();
        var lawyerIds = await SeedUsersAndCaseAsync(context, lawyerCount: 1);
        var selected = CreateProposal(lawyerIds[0], _utcNow.AddHours(-3));
        selected.Accept(_utcNow.AddHours(-2));
        context.Proposals.Add(selected);
        await context.SaveChangesAsync();

        var service = new ContractCaseAssignmentService(
            context,
            new ChatConversationService(
                context,
                new FixedTimeProvider(_utcNow)),
            CreateOutboxWriter(context));
        await service.AssignAsync(
            new ContractCaseAssignment(
                Guid.NewGuid(),
                selected.Id,
                _caseId,
                _clientUserId,
                lawyerIds[0],
                new DateTimeOffset(_utcNow)),
            CancellationToken.None);
        await context.SaveChangesAsync();

        var legalCase = await context.Cases.SingleAsync();
        var conversation = await context.ChatConversations.SingleAsync();
        Assert.Equal(conversation.Id, legalCase.ChatId);
        Assert.Equal(selected.Id, conversation.ProposalId);
        Assert.Equal(_caseId, conversation.LegalCaseId);
        Assert.Equal(_clientUserId, conversation.ClientUserId);
        Assert.Equal(lawyerIds[0], conversation.LawyerUserId);
    }

    [Fact]
    public async Task ContractAssignment_RejectsMismatchedWinningConversation()
    {
        await using var context = CreateContext();
        var lawyerIds = await SeedUsersAndCaseAsync(context, lawyerCount: 2);
        var selected = CreateProposal(lawyerIds[0], _utcNow.AddHours(-3));
        selected.Accept(_utcNow.AddHours(-2));
        var mismatchedConversation = new ChatConversation(
            Guid.NewGuid(),
            selected.Id,
            _caseId,
            _clientUserId,
            lawyerIds[1],
            _utcNow.AddHours(-2));
        context.AddRange(selected, mismatchedConversation);
        await context.SaveChangesAsync();

        var service = new ContractCaseAssignmentService(
            context,
            new ChatConversationService(
                context,
                new FixedTimeProvider(_utcNow)),
            CreateOutboxWriter(context));

        await Assert.ThrowsAsync<SmartCourt.Common.Exceptions.BusinessException>(
            () => service.AssignAsync(
                new ContractCaseAssignment(
                    Guid.NewGuid(),
                    selected.Id,
                    _caseId,
                    _clientUserId,
                    lawyerIds[0],
                    new DateTimeOffset(_utcNow)),
                CancellationToken.None));

        var legalCase = await context.Cases.SingleAsync();
        Assert.Equal(CaseStatus.Matched, legalCase.Status);
        Assert.Null(legalCase.LawyerId);
        Assert.Null(legalCase.ChatId);
    }

    [Fact]
    public async Task ContractAssignment_RejectsClosedWinningConversation()
    {
        await using var context = CreateContext();
        var lawyerIds = await SeedUsersAndCaseAsync(context, lawyerCount: 1);
        var selected = CreateProposal(lawyerIds[0], _utcNow.AddHours(-3));
        selected.Accept(_utcNow.AddHours(-2));
        var closedConversation = new ChatConversation(
            Guid.NewGuid(),
            selected.Id,
            _caseId,
            _clientUserId,
            lawyerIds[0],
            _utcNow.AddHours(-2));
        closedConversation.Close(_utcNow.AddHours(-1));
        context.AddRange(selected, closedConversation);
        await context.SaveChangesAsync();

        var service = new ContractCaseAssignmentService(
            context,
            new ChatConversationService(
                context,
                new FixedTimeProvider(_utcNow)),
            CreateOutboxWriter(context));

        await Assert.ThrowsAsync<SmartCourt.Common.Exceptions.BusinessException>(
            () => service.AssignAsync(
                new ContractCaseAssignment(
                    Guid.NewGuid(),
                    selected.Id,
                    _caseId,
                    _clientUserId,
                    lawyerIds[0],
                    new DateTimeOffset(_utcNow)),
                CancellationToken.None));

        var legalCase = await context.Cases.SingleAsync();
        Assert.Equal(CaseStatus.Matched, legalCase.Status);
        Assert.Null(legalCase.LawyerId);
        Assert.Null(legalCase.ChatId);
    }

    private CreateProposalHandler CreateHandler(ApplicationDbContext context)
    {
        var timeProvider = new FixedTimeProvider(_utcNow);
        var outboxWriter = new OutboxWriter(context, timeProvider);
        return new CreateProposalHandler(
            context,
            new TestCurrentUserService(_clientUserId),
            timeProvider,
            outboxWriter,
            new ProposalExpirationService(
                context,
                outboxWriter,
                timeProvider),
            new CreateProposalCommandValidator());
    }

    private OutboxWriter CreateOutboxWriter(ApplicationDbContext context)
    {
        return new OutboxWriter(context, new FixedTimeProvider(_utcNow));
    }

    private Proposal CreateProposal(Guid lawyerUserId, DateTime createdAt)
    {
        return new Proposal(
            Guid.NewGuid(),
            _caseId,
            _clientUserId,
            lawyerUserId,
            "Please consider representing me in this case.",
            createdAt);
    }

    private async Task<IReadOnlyList<Guid>> SeedUsersAndCaseAsync(
        ApplicationDbContext context,
        int lawyerCount)
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
        context.Users.Add(CreateUser(_clientUserId, "client"));
        context.UserRoles.Add(new IdentityUserRole<Guid>
        {
            UserId = _clientUserId,
            RoleId = clientRole.Id
        });

        var lawyerIds = Enumerable.Range(1, lawyerCount)
            .Select(_ => Guid.NewGuid())
            .ToArray();
        foreach (var (lawyerId, index) in lawyerIds.Select(
                     (id, index) => (id, index)))
        {
            context.Users.Add(CreateUser(lawyerId, $"lawyer{index + 1}"));
            context.UserRoles.Add(new IdentityUserRole<Guid>
            {
                UserId = lawyerId,
                RoleId = lawyerRole.Id
            });
        }

        context.Cases.Add(new SmartCourt.Entities.Case
        {
            Id = _caseId,
            ClientId = _clientUserId,
            Title = "Commercial representation case",
            Description = "A matched case ready for lawyer proposals.",
            Status = CaseStatus.Matched,
            SubmittedAt = _utcNow.AddDays(-1)
        });
        await context.SaveChangesAsync();
        return lawyerIds;
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
            .UseInMemoryDatabase($"proposal-lifecycle-{Guid.NewGuid():N}")
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

    private sealed class RecordingChatNotifier : IChatRealtimeNotifier
    {
        public List<ChatMessageDto> Messages { get; } = [];

        public Task MessageCreatedAsync(
            ChatMessageDto message,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
