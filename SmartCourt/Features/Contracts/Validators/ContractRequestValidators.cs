using FluentValidation;
using SmartCourt.Features.Contracts.DTOs;

namespace SmartCourt.Features.Contracts.Validators;

public sealed class CreateContractRequestValidator
    : AbstractValidator<CreateContractRequest>
{
    public CreateContractRequestValidator()
    {
        RuleFor(request => request.ProposalId)
            .NotEmpty()
            .WithMessage("معرّف العرض مطلوب.");
        RuleFor(request => request.Title)
            .NotEmpty()
            .WithMessage("عنوان العقد مطلوب.")
            .Length(3, 200)
            .WithMessage("عنوان العقد يجب أن يكون بين 3 و200 حرف.");
        RuleFor(request => request.TermsAndConditions)
            .NotEmpty()
            .WithMessage("شروط وأحكام العقد مطلوبة.")
            .Length(20, 20_000)
            .WithMessage(
                "شروط وأحكام العقد يجب أن تكون بين 20 و20000 حرف.");
    }
}

public sealed class UpdateContractRequestValidator
    : AbstractValidator<UpdateContractRequest>
{
    public UpdateContractRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .WithMessage("عنوان العقد مطلوب.")
            .Length(3, 200)
            .WithMessage("عنوان العقد يجب أن يكون بين 3 و200 حرف.");
        RuleFor(request => request.TermsAndConditions)
            .NotEmpty()
            .WithMessage("شروط وأحكام العقد مطلوبة.")
            .Length(20, 20_000)
            .WithMessage(
                "شروط وأحكام العقد يجب أن تكون بين 20 و20000 حرف.");
    }
}

public sealed class TerminateContractRequestValidator
    : AbstractValidator<TerminateContractRequest>
{
    public TerminateContractRequestValidator()
    {
        RuleFor(request => request.Reason)
            .NotEmpty()
            .WithMessage("سبب إنهاء العقد مطلوب.")
            .MaximumLength(2_000)
            .WithMessage("سبب إنهاء العقد يجب ألا يتجاوز 2000 حرف.");
    }
}
