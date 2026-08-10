using System.Globalization;
using System.Text;

namespace SmartCourt.Features.Notifications.Shared;

internal static class NotificationCursor
{
    private const string Prefix = "v1:";
    private const int MaximumCursorLength = 64;

    public static string Encode(long sequence)
    {
        if (sequence <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequence));
        }

        var bytes = Encoding.UTF8.GetBytes(
            $"{Prefix}{sequence.ToString(CultureInfo.InvariantCulture)}");
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static bool TryDecode(string? cursor, out long sequence)
    {
        sequence = 0;
        if (string.IsNullOrWhiteSpace(cursor)
            || cursor.Length > MaximumCursorLength)
        {
            return false;
        }

        try
        {
            var base64 = cursor.Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight(
                base64.Length + (4 - base64.Length % 4) % 4,
                '=');
            var value = Encoding.UTF8.GetString(
                Convert.FromBase64String(base64));
            return value.StartsWith(Prefix, StringComparison.Ordinal)
                && long.TryParse(
                    value[Prefix.Length..],
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out sequence)
                && sequence > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
