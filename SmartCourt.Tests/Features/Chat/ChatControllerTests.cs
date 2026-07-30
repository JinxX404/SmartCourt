using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;
using SmartCourt.Common.Models;
using SmartCourt.Features.Chat;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.GetConversation;
using SmartCourt.Features.Chat.GetConversations;
using SmartCourt.Features.Chat.GetMessages;
using SmartCourt.Features.Chat.SendMessage;
using Xunit;

namespace SmartCourt.Tests.Features.Chat;

public sealed class ChatControllerTests
{
    [Fact]
    public async Task ReadsAndWrites_ReturnWrappedResponses()
    {
        var mediator = new RecordingMediator();
        var controller = new ChatController(mediator);
        var conversationId = Guid.NewGuid();

        var listAction = await controller.ListAsync(
            new GetChatConversationsQuery("case", 2, 10),
            CancellationToken.None);
        var getAction = await controller.GetAsync(
            conversationId,
            CancellationToken.None);
        var messagesAction = await controller.GetMessagesAsync(
            conversationId,
            3,
            25,
            CancellationToken.None);
        var sendAction = await controller.SendMessageAsync(
            conversationId,
            new SendChatMessageRequest("Hello"),
            CancellationToken.None);

        AssertWrappedOk(listAction, mediator.Page);
        AssertWrappedOk(getAction, mediator.Detail);
        AssertWrappedOk(messagesAction, mediator.Messages);
        AssertWrappedOk(sendAction, mediator.Message);
        Assert.Equal("case", mediator.ListQuery!.Search);
        Assert.Equal(2, mediator.ListQuery.Page);
        Assert.Equal(10, mediator.ListQuery.PageSize);
        Assert.Equal(conversationId, mediator.GetQuery!.ConversationId);
        Assert.Equal(conversationId, mediator.MessagesQuery!.ConversationId);
        Assert.Equal(3, mediator.MessagesQuery.Page);
        Assert.Equal(25, mediator.MessagesQuery.PageSize);
        Assert.Equal(conversationId, mediator.SendCommand!.ConversationId);
        Assert.Equal("Hello", mediator.SendCommand.Content);
    }

    [Fact]
    public void Endpoints_DefineExpectedRoutesAndRoles()
    {
        AssertEndpoint(nameof(ChatController.ListAsync));
        AssertEndpoint(nameof(ChatController.GetAsync));
        AssertEndpoint(nameof(ChatController.GetMessagesAsync));
        AssertEndpoint(nameof(ChatController.SendMessageAsync));
    }

    private static void AssertWrappedOk<T>(
        ActionResult<ApiResponse<T>> action,
        ApiResponse<T> expected)
    {
        var result = Assert.IsType<ObjectResult>(Convert(action));
        var response = Assert.IsType<ApiResponse<T>>(result.Value);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Same(expected, response);
    }

    private static IActionResult Convert<T>(ActionResult<ApiResponse<T>> action)
    {
        return ((IConvertToActionResult)action).Convert();
    }

    private static void AssertEndpoint(
        string methodName)
    {
        var method = typeof(ChatController).GetMethod(methodName);
        Assert.NotNull(method);
        var authorize = Assert.Single(
            typeof(ChatController).GetCustomAttributes<AuthorizeAttribute>(
                inherit: true));
        Assert.Equal("Client,Lawyer", authorize.Roles);
    }

    private sealed class RecordingMediator : IMediator
    {
        private readonly ApiResponse<ChatConversationDetailDto> _detail =
            ApiResponse<ChatConversationDetailDto>.Ok(CreateDetail());
        private readonly ApiResponse<ChatConversationPageDto> _page =
            ApiResponse<ChatConversationPageDto>.Ok(
                new ChatConversationPageDto([], 1, 10, 0, false));
        private readonly ApiResponse<ChatMessagePageDto> _messages =
            ApiResponse<ChatMessagePageDto>.Ok(
                new ChatMessagePageDto([], 1, 10, 0, false));
        private readonly ApiResponse<ChatMessageDto> _message =
            ApiResponse<ChatMessageDto>.Ok(CreateMessage());

        public ApiResponse<ChatConversationDetailDto> Detail => _detail;
        public ApiResponse<ChatConversationPageDto> Page => _page;
        public ApiResponse<ChatMessagePageDto> Messages => _messages;
        public ApiResponse<ChatMessageDto> Message => _message;
        public GetChatConversationsQuery? ListQuery { get; private set; }
        public GetChatConversationQuery? GetQuery { get; private set; }
        public GetChatMessagesQuery? MessagesQuery { get; private set; }
        public SendChatMessageCommand? SendCommand { get; private set; }

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                GetChatConversationsQuery query =>
                    Capture(query, _page),
                GetChatConversationQuery query =>
                    Capture(query, _detail),
                GetChatMessagesQuery query =>
                    Capture(query, _messages),
                SendChatMessageCommand command =>
                    Capture(command, _message),
                _ => throw new NotSupportedException(request.GetType().Name)
            };

            return Task.FromResult((TResponse)response);
        }

        public Task<object?> Send(
            object request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(
            object request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;

        private object Capture<TResponse>(
            object capturedRequest,
            TResponse response)
        {
            switch (capturedRequest)
            {
                case GetChatConversationsQuery query:
                    ListQuery = query;
                    break;
                case GetChatConversationQuery query:
                    GetQuery = query;
                    break;
                case GetChatMessagesQuery query:
                    MessagesQuery = query;
                    break;
                case SendChatMessageCommand command:
                    SendCommand = command;
                    break;
            }

            return response!;
        }

        private static ChatConversationDetailDto CreateDetail()
        {
            var participant = new ChatParticipantDto(
                Guid.NewGuid(),
                "Client",
                "Client");
            return new ChatConversationDetailDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Case title",
                participant,
                new ChatParticipantDto(
                    Guid.NewGuid(),
                    "Lawyer",
                    "Lawyer"),
                "Open",
                new DateTime(2026, 7, 30, 9, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 7, 30, 9, 0, 0, DateTimeKind.Utc),
                null);
        }

        private static ChatMessageDto CreateMessage()
        {
            return new ChatMessageDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Client",
                "User",
                "Hello",
                null,
                null,
                new DateTime(2026, 7, 30, 9, 0, 0, DateTimeKind.Utc),
                true);
        }
    }
}
