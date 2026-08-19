using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.Attachments;
using SmartCourt.Features.Chat.Entities;
using SmartCourt.Features.Chat.Events;
using SmartCourt.Features.Chat.GetConversation;
using SmartCourt.Features.Chat.GetConversations;
using SmartCourt.Features.Chat.GetMessages;
using SmartCourt.Features.Chat.Integration;
using SmartCourt.Features.Chat.Realtime;
using SmartCourt.Features.Chat.SendMessage;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Proposals.AcceptProposal;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Proposals.Shared;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using SmartCourt.Tests.TestDoubles;
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
        Assert.Equal(CaseStatus.Matched, (await context.Cases.SingleAsync()).Status);
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
    public async Task GetConversation_ExposesWriteCapabilitiesOnlyForActiveAcceptedConversation()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var conversation = await SeedConversationAsync(context);

        var detailResult = await new GetChatConversationHandler(
            context,
            new MutableCurrentUserService(_clientUserId)).Handle(
                new GetChatConversationQuery(conversation.Id),
                CancellationToken.None);
        var listResult = await new GetChatConversationsHandler(
            context,
            new MutableCurrentUserService(_clientUserId),
            new GetChatConversationsQueryValidator()).Handle(
                new GetChatConversationsQuery(),
                CancellationToken.None);

        Assert.True(detailResult.Success);
        Assert.True(detailResult.Data!.CanSendMessages);
        Assert.True(detailResult.Data.CanUploadAttachments);
        var listItem = Assert.Single(listResult.Data!.Items);
        Assert.True(listItem.CanSendMessages);
        Assert.True(listItem.CanUploadAttachments);
    }

    [Fact]
    public async Task SendAttachments_PersistsReturnsAndBroadcastsSecureMetadata()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var conversation = await SeedConversationAsync(context);
        var notifier = new RecordingNotifier();
        var storage = new TestFileStorageService();
        var fileBytes = "%PDF-1.7\nChat attachment"u8.ToArray();
        var handler = new SendChatAttachmentsHandler(
            context,
            new MutableCurrentUserService(_clientUserId),
            new SendChatAttachmentsCommandValidator(),
            storage,
            notifier,
            new FixedTimeProvider(_utcNow));

        var result = await handler.Handle(
            new SendChatAttachmentsCommand(
                conversation.Id,
                "Evidence for review.",
                [CreateFormFile(fileBytes, "evidence.pdf", "application/pdf")]),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal("Evidence for review.", result.Data!.Content);
        var attachment = Assert.Single(result.Data.Attachments);
        Assert.Equal("evidence.pdf", attachment.FileName);
        Assert.Equal("application/pdf", attachment.ContentType);
        Assert.Equal(fileBytes.Length, attachment.SizeInBytes);
        Assert.Equal(
            $"/api/chat/conversations/{conversation.Id}/attachments/{attachment.Id}/download",
            attachment.DownloadUrl);
        Assert.DoesNotContain("chat-attachments", attachment.DownloadUrl);
        Assert.Single(context.ChatMessageAttachments);
        Assert.Single(context.StoredFiles);

        var realtimeMessage = Assert.Single(notifier.Messages);
        Assert.Equal(result.Data.Id, realtimeMessage.Id);
        Assert.Equal(attachment, Assert.Single(realtimeMessage.Attachments));

        var history = await new GetChatMessagesHandler(
            context,
            new MutableCurrentUserService(_lawyerUserId),
            new GetChatMessagesQueryValidator()).Handle(
                new GetChatMessagesQuery(conversation.Id),
                CancellationToken.None);

        Assert.True(history.Success);
        Assert.Equal(attachment, Assert.Single(
            Assert.Single(history.Data!.Items).Attachments));
    }

    [Fact]
    public async Task SendAttachments_RejectsAFileWhoseContentDoesNotMatchItsExtension()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var conversation = await SeedConversationAsync(context);
        var notifier = new RecordingNotifier();
        var handler = new SendChatAttachmentsHandler(
            context,
            new MutableCurrentUserService(_clientUserId),
            new SendChatAttachmentsCommandValidator(),
            new TestFileStorageService(),
            notifier,
            new FixedTimeProvider(_utcNow));

        var result = await handler.Handle(
            new SendChatAttachmentsCommand(
                conversation.Id,
                null,
                [CreateFormFile("not a PDF"u8.ToArray(), "fake.pdf", "application/pdf")]),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Empty(context.ChatMessages);
        Assert.Empty(context.ChatMessageAttachments);
        Assert.Empty(context.StoredFiles);
        Assert.Empty(notifier.Messages);
    }

    [Fact]
    public async Task AttachmentDownload_EnforcesParticipantAndSupersededPrivacyRules()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var conversation = await SeedConversationAsync(context);
        var storage = new TestFileStorageService
        {
            DownloadBytesToReturn = "private evidence"u8.ToArray()
        };
        var actor = new MutableCurrentUserService(_clientUserId);
        var upload = await new SendChatAttachmentsHandler(
            context,
            actor,
            new SendChatAttachmentsCommandValidator(),
            storage,
            new RecordingNotifier(),
            new FixedTimeProvider(_utcNow)).Handle(
                new SendChatAttachmentsCommand(
                    conversation.Id,
                    null,
                    [CreateFormFile("%PDF-1.7\ndata"u8.ToArray(), "private.pdf", "application/pdf")]),
                CancellationToken.None);
        var attachmentId = Assert.Single(upload.Data!.Attachments).Id;
        var downloadHandler = new DownloadChatAttachmentHandler(
            context,
            actor,
            storage);

        var clientResult = await downloadHandler.Handle(
            new DownloadChatAttachmentQuery(conversation.Id, attachmentId),
            CancellationToken.None);
        Assert.True(clientResult.Success);
        Assert.Equal(storage.DownloadBytesToReturn, clientResult.Data!.Content);

        actor.UserId = _outsiderUserId;
        var outsiderResult = await downloadHandler.Handle(
            new DownloadChatAttachmentQuery(conversation.Id, attachmentId),
            CancellationToken.None);
        Assert.False(outsiderResult.Success);
        Assert.Equal(404, outsiderResult.StatusCode);

        var proposal = await context.Proposals.SingleAsync();
        proposal.Supersede(_utcNow.AddMinutes(1));
        conversation.Close(_utcNow.AddMinutes(1));
        await context.SaveChangesAsync();
        actor.UserId = _lawyerUserId;
        var supersededLawyerResult = await downloadHandler.Handle(
            new DownloadChatAttachmentQuery(conversation.Id, attachmentId),
            CancellationToken.None);
        Assert.False(supersededLawyerResult.Success);
        Assert.Equal(404, supersededLawyerResult.StatusCode);

        actor.UserId = _clientUserId;
        var clientAfterSupersedeResult = await downloadHandler.Handle(
            new DownloadChatAttachmentQuery(conversation.Id, attachmentId),
            CancellationToken.None);
        Assert.True(clientAfterSupersedeResult.Success);
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

    [Fact]
    public async Task SupersededConversation_IsHiddenFromLawyerAcrossChatAccessPaths()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var conversation = await SeedConversationAsync(context);
        var proposal = await context.Proposals.SingleAsync();
        proposal.Supersede(_utcNow);
        conversation.Close(_utcNow);
        context.ChatMessages.Add(
            ChatMessage.CreateUserMessage(
                Guid.NewGuid(),
                conversation.Id,
                _clientUserId,
                "Confidential negotiation details.",
                _utcNow.AddMinutes(-1)));
        await context.SaveChangesAsync();

        var lawyerUser = new MutableCurrentUserService(_lawyerUserId);
        var messagesResult = await new GetChatMessagesHandler(
            context,
            lawyerUser,
            new GetChatMessagesQueryValidator()).Handle(
                new GetChatMessagesQuery(conversation.Id),
                CancellationToken.None);
        var detailResult = await new GetChatConversationHandler(
            context,
            lawyerUser).Handle(
                new GetChatConversationQuery(conversation.Id),
                CancellationToken.None);
        var listResult = await new GetChatConversationsHandler(
            context,
            lawyerUser,
            new GetChatConversationsQueryValidator()).Handle(
                new GetChatConversationsQuery(),
                CancellationToken.None);
        var accessService = new ChatConversationService(
            context,
            new FixedTimeProvider(_utcNow));
        var canJoinSignalRConversation = await accessService.CanAccessConversationAsync(
            conversation.Id,
            _lawyerUserId,
            CancellationToken.None);
        var sendResult = await new SendChatMessageHandler(
            context,
            lawyerUser,
            new SendChatMessageCommandValidator(),
            new RecordingNotifier(),
            new FixedTimeProvider(_utcNow)).Handle(
                new SendChatMessageCommand(
                    conversation.Id,
                    "Attempt to reopen negotiation."),
                CancellationToken.None);

        Assert.False(messagesResult.Success);
        Assert.Equal(404, messagesResult.StatusCode);
        Assert.False(detailResult.Success);
        Assert.Equal(404, detailResult.StatusCode);
        Assert.True(listResult.Success);
        Assert.Empty(listResult.Data!.Items);
        Assert.False(canJoinSignalRConversation);
        Assert.False(sendResult.Success);
        Assert.Equal(404, sendResult.StatusCode);

        lawyerUser.UserId = _clientUserId;
        var clientMessagesResult = await new GetChatMessagesHandler(
            context,
            lawyerUser,
            new GetChatMessagesQueryValidator()).Handle(
                new GetChatMessagesQuery(conversation.Id),
                CancellationToken.None);
        var clientDetailResult = await new GetChatConversationHandler(
            context,
            lawyerUser).Handle(
                new GetChatConversationQuery(conversation.Id),
                CancellationToken.None);
        var clientListResult = await new GetChatConversationsHandler(
            context,
            lawyerUser,
            new GetChatConversationsQueryValidator()).Handle(
                new GetChatConversationsQuery(),
                CancellationToken.None);
        var clientSendResult = await new SendChatMessageHandler(
            context,
            lawyerUser,
            new SendChatMessageCommandValidator(),
            new RecordingNotifier(),
            new FixedTimeProvider(_utcNow)).Handle(
                new SendChatMessageCommand(
                    conversation.Id,
                    "Client should also be read-only."),
                CancellationToken.None);

        Assert.True(clientMessagesResult.Success);
        Assert.Single(clientMessagesResult.Data!.Items);
        Assert.True(clientDetailResult.Success);
        Assert.False(clientDetailResult.Data!.CanSendMessages);
        Assert.False(clientDetailResult.Data.CanUploadAttachments);
        var clientListItem = Assert.Single(clientListResult.Data!.Items);
        Assert.False(clientListItem.CanSendMessages);
        Assert.False(clientListItem.CanUploadAttachments);
        Assert.False(clientSendResult.Success);
        Assert.Equal(409, clientSendResult.StatusCode);
    }

    [Theory]
    [InlineData(ContractStatus.Completed)]
    [InlineData(ContractStatus.Terminated)]
    public async Task TerminalContractConversation_IsHiddenFromLawyerButReadableByClient(
        ContractStatus terminalStatus)
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var conversation = await SeedConversationAsync(context);
        var contract = new Contract(
            Guid.NewGuid(),
            _proposalId,
            _legalCaseId,
            _clientUserId,
            _lawyerUserId,
            "Representation contract",
            "Terms",
            _utcNow.AddMinutes(-10));
        contract.Status = terminalStatus;
        if (terminalStatus == ContractStatus.Completed)
        {
            contract.CompletedAt = _utcNow;
        }
        else
        {
            contract.TerminatedAt = _utcNow;
            contract.TerminatedByUserId = _clientUserId;
        }

        conversation.Close(_utcNow);
        context.Contracts.Add(contract);
        context.ChatMessages.Add(
            ChatMessage.CreateUserMessage(
                Guid.NewGuid(),
                conversation.Id,
                _clientUserId,
                "Private closed-case details.",
                _utcNow.AddMinutes(-1)));
        await context.SaveChangesAsync();

        var lawyerUser = new MutableCurrentUserService(_lawyerUserId);
        var lawyerList = await new GetChatConversationsHandler(
            context,
            lawyerUser,
            new GetChatConversationsQueryValidator()).Handle(
                new GetChatConversationsQuery(),
                CancellationToken.None);
        var lawyerDetail = await new GetChatConversationHandler(
            context,
            lawyerUser).Handle(
                new GetChatConversationQuery(conversation.Id),
                CancellationToken.None);
        var lawyerMessages = await new GetChatMessagesHandler(
            context,
            lawyerUser,
            new GetChatMessagesQueryValidator()).Handle(
                new GetChatMessagesQuery(conversation.Id),
                CancellationToken.None);
        var lawyerSend = await new SendChatMessageHandler(
            context,
            lawyerUser,
            new SendChatMessageCommandValidator(),
            new RecordingNotifier(),
            new FixedTimeProvider(_utcNow)).Handle(
                new SendChatMessageCommand(
                    conversation.Id,
                    "Trying to reopen a terminal contract chat."),
                CancellationToken.None);

        Assert.True(lawyerList.Success);
        Assert.Empty(lawyerList.Data!.Items);
        Assert.False(lawyerDetail.Success);
        Assert.Equal(404, lawyerDetail.StatusCode);
        Assert.False(lawyerMessages.Success);
        Assert.Equal(404, lawyerMessages.StatusCode);
        Assert.False(lawyerSend.Success);
        Assert.Equal(404, lawyerSend.StatusCode);

        lawyerUser.UserId = _clientUserId;
        var clientDetail = await new GetChatConversationHandler(
            context,
            lawyerUser).Handle(
                new GetChatConversationQuery(conversation.Id),
                CancellationToken.None);
        var clientMessages = await new GetChatMessagesHandler(
            context,
            lawyerUser,
            new GetChatMessagesQueryValidator()).Handle(
                new GetChatMessagesQuery(conversation.Id),
                CancellationToken.None);

        Assert.True(clientDetail.Success);
        Assert.False(clientDetail.Data!.CanSendMessages);
        Assert.False(clientDetail.Data.CanUploadAttachments);
        Assert.True(clientMessages.Success);
        Assert.Single(clientMessages.Data!.Items);
    }

    [Theory]
    [InlineData(ContractConversationMessageType.ContractCompleted)]
    [InlineData(ContractConversationMessageType.ContractTerminated)]
    public async Task TerminalContractEvent_ClosesConversationAndBlocksMessages(
        ContractConversationMessageType messageType)
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var conversation = await SeedConversationAsync(context);
        var notifier = new RecordingNotifier();
        var service = new ContractConversationService(
            context,
            new ChatConversationService(
                context,
                new FixedTimeProvider(_utcNow)),
            notifier);

        await service.AppendSystemMessageAsync(
            new ContractConversationSystemMessage(
                Guid.NewGuid(),
                _proposalId,
                messageType,
                Guid.NewGuid(),
                new DateTimeOffset(_utcNow)),
            CancellationToken.None);

        Assert.True(conversation.IsClosed);
        Assert.Contains(
            context.ChatMessages,
            message => message.SystemCode == messageType.ToString());

        var sendResult = await new SendChatMessageHandler(
            context,
            new MutableCurrentUserService(_clientUserId),
            new SendChatMessageCommandValidator(),
            notifier,
            new FixedTimeProvider(_utcNow)).Handle(
                new SendChatMessageCommand(
                    conversation.Id,
                    "This conversation is now read-only."),
                CancellationToken.None);

        Assert.False(sendResult.Success);
        Assert.Equal(409, sendResult.StatusCode);
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
        context.Cases.Add(
            new SmartCourt.Entities.Case { Id = _legalCaseId, ClientId = _clientUserId, Title = "Case title", Description = "Case description", City = "Cairo", SubmittedAt = _utcNow.AddHours(-1), Status = CaseStatus.Submitted });
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
        var legalCase = new SmartCourt.Entities.Case
        {
            Id = _legalCaseId,
            ClientId = _clientUserId,
            Title = "Case title",
            Description = "Case description",
            Status = CaseStatus.Matched,
            SubmittedAt = _utcNow.AddHours(-2)
        };
        var proposal = new Proposal(
            _proposalId,
            _legalCaseId,
            _clientUserId,
            _lawyerUserId,
            "Please help.",
            _utcNow.AddHours(-2));
        proposal.Accept(_utcNow.AddHours(-1));
        var conversation = new ChatConversation(
            Guid.NewGuid(),
            _proposalId,
            _legalCaseId,
            _clientUserId,
            _lawyerUserId,
            _utcNow.AddMinutes(-15));
        context.AddRange(legalCase, proposal, conversation);
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

    private static IFormFile CreateFormFile(
        byte[] content,
        string fileName,
        string contentType)
    {
        return new FormFile(
            new MemoryStream(content),
            0,
            content.Length,
            "files",
            fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
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

