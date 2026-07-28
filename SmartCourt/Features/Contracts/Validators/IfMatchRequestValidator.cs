using FluentValidation;
using SmartCourt.Features.Contracts.DTOs;

namespace SmartCourt.Features.Contracts.Validators;

public sealed class IfMatchRequestValidator
    : AbstractValidator<IfMatchRequest>
{
    public IfMatchRequestValidator()
    {
        RuleFor(request => request.IfMatch)
            .NotEmpty()
            .WithMessage("قيمة If-Match مطلوبة.")
            .Must(IsStrongBase64Etag)
            .WithMessage(
                "قيمة If-Match يجب أن تكون وسم ETag قويًا يحتوي على rowversion مشفّر بصيغة base64 بين علامتي اقتباس.");
    }

    private static bool IsStrongBase64Etag(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Length < 3
            || value[0] != '"'
            || value[^1] != '"'
            || value.StartsWith("W/\"", StringComparison.Ordinal))
        {
            return false;
        }

        var encoded = value[1..^1];
        if (encoded.Length == 0)
        {
            return false;
        }

        try
        {
            return Convert.FromBase64String(encoded).Length > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
