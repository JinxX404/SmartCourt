using SmartCourt.Common.Exceptions;

namespace SmartCourt.Common.Domain;

internal static class EntityGuard
{
    public const string CurrencyEgp = "EGP";

    public static Guid NotEmpty(Guid value, string fieldName)
    {
        if (value == Guid.Empty)
        {
            throw new BusinessException($"{fieldName} is required.");
        }

        return value;
    }

    public static Guid? OptionalGuid(Guid? value, string fieldName)
    {
        if (value == Guid.Empty)
        {
            throw new BusinessException($"{fieldName} must not be empty when supplied.");
        }

        return value;
    }

    public static string Required(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessException($"{fieldName} is required.");
        }

        return value;
    }

    public static DateTime Utc(DateTime value, string fieldName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new BusinessException($"{fieldName} must be UTC.");
        }

        return value;
    }

    public static DateTime? OptionalUtc(DateTime? value, string fieldName)
    {
        if (value.HasValue)
        {
            Utc(value.Value, fieldName);
        }

        return value;
    }

    public static decimal PositiveMoney(decimal value, string fieldName)
    {
        if (value <= 0)
        {
            throw new BusinessException($"{fieldName} must be greater than zero.");
        }

        if (decimal.Round(value, 2) != value)
        {
            throw new BusinessException($"{fieldName} must have no more than two decimal places.");
        }

        return value;
    }

    public static decimal NonNegativeMoney(decimal value, string fieldName)
    {
        if (value < 0)
        {
            throw new BusinessException($"{fieldName} must not be negative.");
        }

        if (decimal.Round(value, 2) != value)
        {
            throw new BusinessException($"{fieldName} must have no more than two decimal places.");
        }

        return value;
    }

    public static int Positive(int value, string fieldName)
    {
        if (value <= 0)
        {
            throw new BusinessException($"{fieldName} must be greater than zero.");
        }

        return value;
    }
}
