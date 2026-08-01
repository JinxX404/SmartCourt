using FluentValidation;
using SmartCourt.Features.Disputes.DTOs;

namespace SmartCourt.Features.Disputes.Validators;

public sealed class AddDisputeEvidenceRequestValidator
    : AbstractValidator<AddDisputeEvidenceRequest>
{
    public AddDisputeEvidenceRequestValidator()
    {
        RuleFor(request => request.Content)
            .MaximumLength(20_000)
            .WithMessage("محتوى الدليل يجب ألا يتجاوز 20000 حرف.");
        RuleFor(request => request.StoredFileIds)
            .NotNull()
            .WithMessage("قائمة ملفات الأدلة مطلوبة.")
            .Must(ids => ids is not null
                && ids.All(id => id != Guid.Empty)
                && ids.Distinct().Count() == ids.Count)
            .WithMessage("قائمة ملفات الأدلة تحتوي على معرّف غير صالح أو مكرر.");
        RuleFor(request => request)
            .Must(request => !string.IsNullOrWhiteSpace(request.Content)
                || request.StoredFileIds.Count > 0)
            .WithMessage("يجب إرفاق ملف أو كتابة محتوى لإضافة دليل إلى النزاع.");
    }
}
