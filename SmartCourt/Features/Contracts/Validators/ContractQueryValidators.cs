using FluentValidation;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Contracts.Enums;

namespace SmartCourt.Features.Contracts.Validators;

public sealed class ContractListQueryValidator
    : AbstractValidator<ContractListQuery>
{
    public ContractListQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("رقم الصفحة يجب أن يكون 1 أو أكبر.");
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("حجم الصفحة يجب أن يكون بين 1 و100.");
        RuleFor(query => query.Status)
            .Must(status =>
                !status.HasValue
                || Enum.IsDefined(typeof(ContractStatus), status.Value))
            .WithMessage("حالة العقد المحددة غير صالحة.");
    }
}

public sealed class ContractStateHistoryQueryValidator
    : AbstractValidator<ContractStateHistoryQuery>
{
    public ContractStateHistoryQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("رقم الصفحة يجب أن يكون 1 أو أكبر.");
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("حجم الصفحة يجب أن يكون بين 1 و100.");
    }
}
