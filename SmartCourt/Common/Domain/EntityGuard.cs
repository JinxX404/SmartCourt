using SmartCourt.Common.Exceptions;

namespace SmartCourt.Common.Domain;

internal static class EntityGuard
{
    public const string CurrencyEgp = "EGP";

    public static Guid NotEmpty(Guid value, string fieldName)
    {
        if (value == Guid.Empty)
        {
            throw new BusinessException($"الحقل {fieldName} مطلوب.");
        }

        return value;
    }

    public static Guid? OptionalGuid(Guid? value, string fieldName)
    {
        if (value == Guid.Empty)
        {
            throw new BusinessException(
                $"يجب ألا تكون قيمة الحقل {fieldName} فارغة عند إدخالها.");
        }

        return value;
    }

    public static string Required(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessException($"الحقل {fieldName} مطلوب.");
        }

        return value;
    }

    public static DateTime Utc(DateTime value, string fieldName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new BusinessException(
                $"يجب أن تكون قيمة الحقل {fieldName} بالتوقيت العالمي المنسق.");
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
            throw new BusinessException(
                $"يجب أن تكون قيمة الحقل {fieldName} أكبر من صفر.");
        }

        if (decimal.Round(value, 2) != value)
        {
            throw new BusinessException(
                $"يجب ألا تتجاوز قيمة الحقل {fieldName} منزلتين عشريتين.");
        }

        return value;
    }

    public static decimal NonNegativeMoney(decimal value, string fieldName)
    {
        if (value < 0)
        {
            throw new BusinessException(
                $"يجب ألا تكون قيمة الحقل {fieldName} سالبة.");
        }

        if (decimal.Round(value, 2) != value)
        {
            throw new BusinessException(
                $"يجب ألا تتجاوز قيمة الحقل {fieldName} منزلتين عشريتين.");
        }

        return value;
    }

    public static int Positive(int value, string fieldName)
    {
        if (value <= 0)
        {
            throw new BusinessException(
                $"يجب أن تكون قيمة الحقل {fieldName} أكبر من صفر.");
        }

        return value;
    }
}
