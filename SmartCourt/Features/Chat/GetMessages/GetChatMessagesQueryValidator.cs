using FluentValidation;

namespace SmartCourt.Features.Chat.GetMessages;

public sealed class GetChatMessagesQueryValidator
    : AbstractValidator<GetChatMessagesQuery>
{
    public GetChatMessagesQueryValidator()
    {
        RuleFor(query => query.ConversationId)
            .NotEmpty();
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);
    }
}
