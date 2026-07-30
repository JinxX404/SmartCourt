using FluentValidation;

namespace SmartCourt.Features.Chat.GetConversations;

public sealed class GetChatConversationsQueryValidator
    : AbstractValidator<GetChatConversationsQuery>
{
    public GetChatConversationsQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 50);
        RuleFor(query => query.Search)
            .MaximumLength(200);
    }
}
