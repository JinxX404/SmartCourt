using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.Hubs;
using Xunit;

namespace SmartCourt.Tests.Features.Chat;

public sealed class ChatHubContractTests
{
    [Theory]
    [InlineData(nameof(ChatHub.JoinConversation), 1)]
    [InlineData(nameof(ChatHub.LeaveConversation), 1)]
    [InlineData(nameof(ChatHub.SendMessage), 2)]
    public void ClientInvokableMethods_ExposeOnlyFrontendArguments(
        string methodName,
        int expectedParameterCount)
    {
        var method = typeof(ChatHub).GetMethod(methodName);

        Assert.NotNull(method);
        Assert.Equal(expectedParameterCount, method.GetParameters().Length);
        Assert.DoesNotContain(
            method.GetParameters(),
            parameter => parameter.ParameterType == typeof(CancellationToken));
    }

    [Fact]
    public void ClientEvent_UsesTheSharedMessageDto()
    {
        var method = typeof(IChatClient).GetMethod(nameof(IChatClient.ReceiveMessage));

        Assert.NotNull(method);
        var parameter = Assert.Single(method.GetParameters());
        Assert.Equal(typeof(ChatMessageDto), parameter.ParameterType);
    }
}
