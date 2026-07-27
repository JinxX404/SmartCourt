using SmartCourt.Common.Exceptions;

namespace SmartCourt.Infrastructure.Idempotency;

public static class IdempotencyHeader
{
    public const string Name = "Idempotency-Key";
    public const int MaximumLength = 200;

    public static string Require(string? value)
    {
        var key = value?.Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new BusinessException(
                "Idempotency-Key header is required.");
        }

        if (key.Length > MaximumLength)
        {
            throw new BusinessException(
                $"Idempotency-Key header must not exceed {MaximumLength} characters.");
        }

        return key;
    }
}
