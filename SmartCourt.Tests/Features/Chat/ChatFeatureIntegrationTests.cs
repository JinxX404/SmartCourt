using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Cases.Entities;
using SmartCourt.Features.Cases.Enums;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.Entities;
using SmartCourt.Features.Chat.Events;
using SmartCourt.Features.Chat.GetMessages;
using SmartCourt.Features.Chat.Integration;
using SmartCourt.Features.Chat.Realtime;
using SmartCourt.Features.Chat.SendMessage;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Features.Proposals.AcceptProposal;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Proposals.Shared;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Chat;

public sealed class ChatFeatureIntegrationTests
{
    private readonly DateTime _utcNow =
        new(2026, 7, 30, 10, 0, 0, DateTimeKind.Utc);
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _lawyerUserId = Guid.NewGuid();
    private readonly Guid _outsiderUserId = Guid.NewGuid();
    private readonly Guid _proposalId = Guid.NewGuid();
    private readonly Guid _legalCaseId = Guid.NewGuid();

    [Fact]
    public async Task AcceptProposal_CreatesConversationAndReturnsConversationId()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        await SeedAcceptedProposalAsync(context);

        var handler = new AcceptProposalHandler(
            context,
            new MutableCurrentUserService(_lawyerUserId),
            new FixedTimeProvider(_utcNow),
            new OutboxWriter(context, new FixedTimeProvider(_utcNow)),
            new ChatConversationService(
                context,
                new FixedTimeProvider(_utcNow)));

        var result = await handler.Handle(
            new AcceptProposalCommand(_proposalId),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(_proposalId, result.Data!.Id);
        Assert.NotNull(result.Data.ConversationId);
        var conversation = await context.ChatConversations.SingleAsync();
        Assert.Equal(result.Data.ConversationId, conversation.Id);
        Assert.Equal(ProposalStatus.Accepted, (await context.Proposals.SingleAsync()).Status);
        Assert.Equal(CaseStatus.Matched, (await context.LegalCases.SingleAsync()).Status);
    }

    [Fact]
    public async Task SendMessage_PersistsAndBroadcastsToParticipants()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var conversation = await SeedConversationAsync(context);
        var notifier = new RecordingNotifier();
        var handler = new SendChatMessageHandler(
            context,
            new MutableCurrentUserService(_clientUserId),
            new SendChatMessageCommandValidator(),
            notifier,
            new FixedTimeProvider(_utcNow));

        var result = await handler.Handle(
            new SendChatMessageCommand(conversation.Id, "Hello, lawyer."),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Hello, lawyer.", result.Data!.Content);
        Assert.Equal(_clientUserId, result.Data.SenderUserId);
        Assert.True(result.Data.IsMine);
        Assert.Single(context.ChatMessages);
        Assert.Single(notifier.Messages);
        Assert.Equal(conversation.Id, notifier.Messages[0].ConversationId);
        Assert.True(notifier.Messages[0].IsMine);
        Assert.Equal(_clientUserId, notifier.Messages[0].SenderUserId);
        Assert.Equal("Hello, lawyer.", notifier.Messages[0].Content);
    }

    [Fact]
    public async Task GetMessages_RejectsOutsider()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var conversation = await SeedConversationAsync(context);
        context.ChatMessages.Add(
            ChatMessage.CreateUserMessage(
                Guid.NewGuid(),
                conversation.Id,
                _clientUserId,
                "Hello",
                _utcNow));
        await context.SaveChangesAsync();

        var handler = new GetChatMessagesHandler(
            context,
            new MutableCurrentUserService(_outsiderUserId),
            new GetChatMessagesQueryValidator());

        var result = await handler.Handle(
            new GetChatMessagesQuery(conversation.Id),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task GetMessages_ReturnsChronologicalItemsForParticipant()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var conversation = await SeedConversationAsync(context);
        context.ChatMessages.Add(
            ChatMessage.CreateUserMessage(
                Guid.NewGuid(),
                conversation.Id,
                _clientUserId,
                "First",
                _utcNow.AddMinutes(-5)));
        context.ChatMessages.Add(
            ChatMessage.CreateSystemMessage(
                Guid.NewGuid(),
                conversation.Id,
                ContractConversationMessageType.ContractAccepted,
                Guid.NewGuid(),
                _utcNow.AddMinutes(-1)));
        await context.SaveChangesAsync();

        var handler = new GetChatMessagesHandler(
            context,
            new MutableCurrentUserService(_clientUserId),
            new GetChatMessagesQueryValidator());

        var result = await handler.Handle(
            new GetChatMessagesQuery(conversation.Id),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Items.Count);
        Assert.Equal("First", result.Data.Items[0].Content);
        Assert.True(result.Data.Items[0].IsMine);
        Assert.Equal("Contract draft was accepted.", result.Data.Items[1].Content);
        Assert.False(result.Data.Items[1].IsMine);
    }

    private async Task SeedUsersAsync(ApplicationDbContext context)
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
            CreateUser(_clientUserId, "client", "Client Name"),
            CreateUser(_lawyerUserId, "lawyer", "Lawyer Name"),
            CreateUser(_outsiderUserId, "outsider", "Outsider Name"));
        context.UserRoles.AddRange(
            new IdentityUserRole<Guid>
            {
                UserId = _clientUserId,
                RoleId = clientRole.Id
            },
            new IdentityUserRole<Guid>
            {
                UserId = _lawyerUserId,
                RoleId = lawyerRole.Id
            });
        await context.SaveChangesAsync();
    }

    private async Task SeedAcceptedProposalAsync(ApplicationDbContext context)
    {
        context.LegalCases.Add(
            new LegalCase(
                _legalCaseId,
                _clientUserId,
                "Case title",
                "Case description",
                "Cairo",
                _utcNow.AddHours(-1))
            {
                Status = CaseStatus.Submitted
            });
        context.Proposals.Add(
            new Proposal(
                _proposalId,
                _legalCaseId,
                _clientUserId,
                _lawyerUserId,
                "Please help.",
                _utcNow.AddMinutes(-30)));
        await context.SaveChangesAsync();
    }

    private async Task<ChatConversation> SeedConversationAsync(
        ApplicationDbContext context)
    {
        var conversation = new ChatConversation(
            Guid.NewGuid(),
            _proposalId,
            _legalCaseId,
            _clientUserId,
            _lawyerUserId,
            _utcNow.AddMinutes(-15));
        context.ChatConversations.Add(conversation);
        await context.SaveChangesAsync();
        return conversation;
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"chat-tests-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(
            options,
            new FixedTimeProvider(_utcNow));
    }

    private static ApplicationUser CreateUser(
        Guid id,
        string userName,
        string fullName)
    {
        return new ApplicationUser
        {
            Id = id,
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = $"{userName}@example.test",
            NormalizedEmail = $"{userName}@example.test"
                .ToUpperInvariant(),
            FullName = fullName,
            NationalNumber = id.ToString("N")[..14]
        };
    }

    private sealed class MutableCurrentUserService(Guid userId)
        : ICurrentUserService
    {
        public Guid? UserId { get; set; } = userId;
        public bool IsAuthenticated => UserId.HasValue;
    }

    private sealed class FixedTimeProvider(DateTime utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return new DateTimeOffset(utcNow);
        }
    }

    private sealed class RecordingNotifier : IChatRealtimeNotifier
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
