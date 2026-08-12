namespace SmartCourt.Features.Chat.DTOs;

public sealed record SendChatMessageRequest(string Content);

public sealed class SendChatAttachmentsRequest
{
    public string? Caption { get; init; }
    public IReadOnlyList<IFormFile> Files { get; init; } = [];
}
