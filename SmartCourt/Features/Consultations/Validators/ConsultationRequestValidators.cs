using FluentValidation;
using SmartCourt.Features.Consultations.Domain.Enums;
using SmartCourt.Features.Consultations.DTOs;
using SmartCourt.Features.Consultations.Shared;

namespace SmartCourt.Features.Consultations.Validators;

public sealed class UpdateConsultationSettingsRequestValidator
    : AbstractValidator<UpdateConsultationSettingsRequest>
{
    public UpdateConsultationSettingsRequestValidator()
    {
        RuleFor(item => item.MinimumBookingNoticeHours).InclusiveBetween(0, 168);
        RuleFor(item => item.MaximumAdvanceBookingDays).InclusiveBetween(1, 365);
        RuleFor(item => item.BufferMinutes).InclusiveBetween(0, 120);
        RuleFor(item => item.TimeZoneId).NotEmpty().MaximumLength(100)
            .Must(BeTimeZone).WithMessage("A valid time-zone identifier is required.");
    }

    private static bool BeTimeZone(string value)
    {
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(value);
            return true;
        }
        catch (TimeZoneNotFoundException) { return false; }
        catch (InvalidTimeZoneException) { return false; }
    }
}

public sealed class CreateConsultationOfferingRequestValidator
    : AbstractValidator<CreateConsultationOfferingRequest>
{
    public CreateConsultationOfferingRequestValidator()
    {
        RuleFor(item => item.Mode).IsInEnum().NotEqual((ConsultationMode)0);
        RuleFor(item => item.Specialization).IsInEnum();
        RuleFor(item => item.Title).NotEmpty().MinimumLength(5).MaximumLength(120);
        RuleFor(item => item.Description).NotEmpty().MinimumLength(20).MaximumLength(2_000);
        RuleFor(item => item.DurationMinutes).InclusiveBetween(15, 240);
        RuleFor(item => item.Price).GreaterThan(0).LessThanOrEqualTo(100_000)
            .Must(value => decimal.Round(value, 2) == value)
            .WithMessage("Price can have at most two decimal places.");
        RuleFor(item => item.OfficeLocation).NotEmpty().MaximumLength(500)
            .When(item => item.Mode == ConsultationMode.InOffice);
        RuleFor(item => item.OfficeLocation).Empty()
            .When(item => item.Mode != ConsultationMode.InOffice)
            .WithMessage("Office location is only accepted for in-office consultations.");
        RuleFor(item => item.Inclusions).NotNull().Must(items => items.Count is >= 1 and <= 10)
            .WithMessage("Provide between one and ten included items.");
        RuleForEach(item => item.Inclusions).NotEmpty().MaximumLength(200);
        RuleFor(item => item.Inclusions).Must(items => items
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase).Count() == items.Count)
            .WithMessage("Included items must be unique.");
    }
}

public sealed class UpdateConsultationOfferingRequestValidator
    : AbstractValidator<UpdateConsultationOfferingRequest>
{
    public UpdateConsultationOfferingRequestValidator()
    {
        Include(new CreateConsultationOfferingRequestValidatorAdapter());
    }

    private sealed class CreateConsultationOfferingRequestValidatorAdapter
        : AbstractValidator<UpdateConsultationOfferingRequest>
    {
        public CreateConsultationOfferingRequestValidatorAdapter()
        {
            RuleFor(item => new CreateConsultationOfferingRequest(
                    item.Mode, item.Specialization, item.Title, item.Description,
                    item.DurationMinutes, item.Price, item.OfficeLocation,
                    item.Inclusions, false))
                .SetValidator(new CreateConsultationOfferingRequestValidator());
        }
    }
}

public sealed class CreateConsultationSlotsRequestValidator
    : AbstractValidator<CreateConsultationSlotsRequest>
{
    public CreateConsultationSlotsRequestValidator()
    {
        RuleFor(item => item.Slots).NotNull().Must(items => items.Count is >= 1 and <= 100)
            .WithMessage("Create between one and 100 slots at a time.");
        RuleForEach(item => item.Slots).ChildRules(slot => slot
            .RuleFor(item => item.StartAtUtc)
            .Must(value => value.Kind == DateTimeKind.Utc)
            .WithMessage("Slot start times must be UTC."));
        RuleFor(item => item.Slots).Must(items => items.Select(slot => slot.StartAtUtc).Distinct().Count() == items.Count)
            .WithMessage("Slot start times must be unique.");
    }
}

public sealed class CreateConsultationBookingRequestValidator
    : AbstractValidator<CreateConsultationBookingRequest>
{
    public CreateConsultationBookingRequestValidator()
    {
        RuleFor(item => item.OfferingId).NotEmpty();
        RuleFor(item => item.SlotId).NotEmpty();
        RuleFor(item => item.Subject).NotEmpty().MinimumLength(5).MaximumLength(150);
        RuleFor(item => item.MatterSummary).NotEmpty().MinimumLength(20).MaximumLength(3_000);
    }
}

public sealed class CreateConsultationPaymentSessionRequestValidator
    : AbstractValidator<CreateConsultationPaymentSessionRequest>
{
    public CreateConsultationPaymentSessionRequestValidator() =>
        RuleFor(item => item.ConfirmationTokenReference).NotEmpty().MaximumLength(500);
}

public sealed class CancelConsultationBookingRequestValidator
    : AbstractValidator<CancelConsultationBookingRequest>
{
    public CancelConsultationBookingRequestValidator() =>
        RuleFor(item => item.Reason).NotEmpty().MinimumLength(5).MaximumLength(1_000);
}

public sealed class OpenConsultationDisputeRequestValidator
    : AbstractValidator<OpenConsultationDisputeRequest>
{
    public OpenConsultationDisputeRequestValidator() =>
        RuleFor(item => item.Reason).NotEmpty().MinimumLength(20).MaximumLength(2_000);
}

public sealed class SetConsultationDeliveryDetailsRequestValidator
    : AbstractValidator<SetConsultationDeliveryDetailsRequest>
{
    public SetConsultationDeliveryDetailsRequestValidator() =>
        RuleFor(item => item.MeetingUrl).NotEmpty().MaximumLength(1_000)
            .Must(value => Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && uri.Scheme == Uri.UriSchemeHttps)
            .WithMessage("A valid HTTPS meeting URL is required.");
}

public sealed class MarkConsultationPerformedRequestValidator
    : AbstractValidator<MarkConsultationPerformedRequest>
{
    public MarkConsultationPerformedRequestValidator() =>
        RuleFor(item => item.MeetingUrl).MaximumLength(1_000)
            .Must(value => string.IsNullOrWhiteSpace(value)
                || Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && uri.Scheme == Uri.UriSchemeHttps)
            .WithMessage("Meeting URL must be a valid HTTPS URL when provided.");
}

public sealed class SettleConsultationDisputeRequestValidator
    : AbstractValidator<SettleConsultationDisputeRequest>
{
    public SettleConsultationDisputeRequestValidator()
    {
        RuleFor(item => item.ClientRefundAmount).GreaterThanOrEqualTo(0)
            .Must(value => decimal.Round(value, 2) == value);
        RuleFor(item => item.Reason).NotEmpty().MinimumLength(10).MaximumLength(1_000);
    }
}

public sealed class ConsultationLawyerFilterValidator
    : AbstractValidator<ConsultationLawyerFilter>
{
    public ConsultationLawyerFilterValidator()
    {
        RuleFor(item => item.Page).GreaterThanOrEqualTo(1);
        RuleFor(item => item.PageSize).InclusiveBetween(1, ConsultationPolicy.MaximumPageSize);
        RuleFor(item => item.MinimumPrice).GreaterThanOrEqualTo(0).When(item => item.MinimumPrice.HasValue);
        RuleFor(item => item.MaximumPrice).GreaterThan(0).When(item => item.MaximumPrice.HasValue);
        RuleFor(item => item).Must(item => !item.MinimumPrice.HasValue || !item.MaximumPrice.HasValue
                || item.MinimumPrice <= item.MaximumPrice)
            .WithMessage("Minimum price cannot exceed maximum price.");
        RuleFor(item => item).Must(item => !item.AvailableFromUtc.HasValue || !item.AvailableToUtc.HasValue
                || item.AvailableFromUtc <= item.AvailableToUtc)
            .WithMessage("Availability start cannot be after availability end.");
        RuleFor(item => item.Search).MaximumLength(100);
    }
}

public sealed class ConsultationBookingFilterValidator
    : AbstractValidator<ConsultationBookingFilter>
{
    public ConsultationBookingFilterValidator()
    {
        RuleFor(item => item.Page).GreaterThanOrEqualTo(1);
        RuleFor(item => item.PageSize).InclusiveBetween(1, ConsultationPolicy.MaximumPageSize);
        RuleFor(item => item).Must(item => !item.FromUtc.HasValue || !item.ToUtc.HasValue || item.FromUtc <= item.ToUtc)
            .WithMessage("The start date cannot be after the end date.");
    }
}
