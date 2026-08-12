using System;
using FluentValidation.TestHelper;
using SmartCourt.Features.ChatAgent.DTOs;
using SmartCourt.Features.ChatAgent.Validators;
using Xunit;

namespace SmartCourt.Tests.Features.ChatAgent;

public sealed class ChatAgentValidatorTests
{
    private readonly CreateAgentConversationRequestValidator _createValidator = new();
    private readonly SendAgentMessageRequestValidator _sendMessageValidator = new();

    [Fact]
    public void CreateConversationRequest_NullCaseId_PassesValidation()
    {
        var request = new CreateAgentConversationRequest(CaseId: null);
        var result = _createValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateConversationRequest_ValidCaseId_PassesValidation()
    {
        var request = new CreateAgentConversationRequest(CaseId: Guid.NewGuid());
        var result = _createValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateConversationRequest_EmptyGuidCaseId_FailsValidation()
    {
        var request = new CreateAgentConversationRequest(CaseId: Guid.Empty);
        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CaseId);
    }

    [Fact]
    public void SendMessageRequest_ValidContent_PassesValidation()
    {
        var request = new SendAgentMessageRequest("ما هو التكييف القانوني للواقعة؟");
        var result = _sendMessageValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SendMessageRequest_EmptyContent_FailsValidation()
    {
        var request = new SendAgentMessageRequest("");
        var result = _sendMessageValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void SendMessageRequest_ContentExceeding2000Chars_FailsValidation()
    {
        var longContent = new string('أ', 2001);
        var request = new SendAgentMessageRequest(longContent);
        var result = _sendMessageValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }
}
