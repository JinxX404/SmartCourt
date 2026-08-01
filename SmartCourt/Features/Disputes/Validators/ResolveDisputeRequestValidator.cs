using FluentValidation;
using SmartCourt.Features.Disputes.DTOs;
using SmartCourt.Features.Disputes.Enums;

namespace SmartCourt.Features.Disputes.Validators;

public sealed class ResolveDisputeRequestValidator
    : AbstractValidator<ResolveDisputeRequest>
{
    public ResolveDisputeRequestValidator()
    {
        RuleFor(request => request.ResolutionType)
            .IsInEnum()
            .WithMessage("نوع تسوية النزاع غير صالح.");
        RuleFor(request => request.ClientRefundAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("مبلغ رد العميل لا يمكن أن يكون سالبًا.")
            .Must(HasAtMostTwoDecimalPlaces)
            .WithMessage("مبلغ رد العميل يجب ألا يتجاوز منزلتين عشريتين.");
        RuleFor(request => request.LawyerReleaseAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("صافي المبلغ المحرر للمحامي لا يمكن أن يكون سالبًا.")
            .Must(HasAtMostTwoDecimalPlaces)
            .WithMessage("صافي المبلغ المحرر للمحامي يجب ألا يتجاوز منزلتين عشريتين.");
        RuleFor(request => request.Summary)
            .NotEmpty()
            .WithMessage("ملخص قرار النزاع مطلوب.")
            .MaximumLength(2_000)
            .WithMessage("ملخص قرار النزاع يجب ألا يتجاوز 2000 حرف.");
        RuleFor(request => request.PenaltyType)
            .Must(value => !value.HasValue || Enum.IsDefined(value.Value))
            .WithMessage("نوع العقوبة المحددة غير صالح.");
        RuleFor(request => request.PenaltyReason)
            .NotEmpty()
            .When(request => request.PenaltyType.HasValue)
            .WithMessage("سبب العقوبة مطلوب عند تحديد عقوبة.")
            .MaximumLength(2_000)
            .WithMessage("سبب العقوبة يجب ألا يتجاوز 2000 حرف.");
        RuleFor(request => request.PenaltyReason)
            .Empty()
            .When(request => !request.PenaltyType.HasValue)
            .WithMessage("لا يمكن إدخال سبب عقوبة دون تحديد نوع العقوبة.");
        RuleFor(request => request)
            .Must(HaveValidOutcomeAmounts)
            .WithMessage("مبالغ التسوية لا تتوافق مع نوع قرار النزاع المحدد.");
    }

    private static bool HaveValidOutcomeAmounts(ResolveDisputeRequest request)
        => request.ResolutionType switch
        {
            DisputeResolutionType.FullRefund =>
                request.ClientRefundAmount > 0m
                && request.LawyerReleaseAmount == 0m,
            DisputeResolutionType.FullRelease =>
                request.ClientRefundAmount == 0m
                && request.LawyerReleaseAmount > 0m,
            DisputeResolutionType.PartialSplit =>
                request.ClientRefundAmount > 0m
                && request.LawyerReleaseAmount > 0m,
            _ => false
        };

    private static bool HasAtMostTwoDecimalPlaces(decimal amount)
        => decimal.Round(amount, 2, MidpointRounding.AwayFromZero) == amount;
}
