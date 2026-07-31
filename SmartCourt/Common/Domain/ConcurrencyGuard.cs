using System.Security.Cryptography;
using SmartCourt.Common.Exceptions;

namespace SmartCourt.Common.Domain;

public static class ConcurrencyGuard
{
    public static byte[] ParseIfMatch(
        string ifMatch,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(ifMatch)
            || ifMatch.Length < 3
            || ifMatch[0] != '"'
            || ifMatch[^1] != '"'
            || ifMatch.StartsWith("W/\"", StringComparison.Ordinal))
        {
            throw new BusinessException(errorMessage);
        }

        try
        {
            var rowVersion = Convert.FromBase64String(ifMatch[1..^1]);
            return rowVersion.Length > 0
                ? rowVersion
                : throw new BusinessException(errorMessage);
        }
        catch (FormatException exception)
        {
            throw new BusinessException(errorMessage, exception);
        }
    }

    public static void EnsureExpectedVersion(
        byte[] currentRowVersion,
        byte[] expectedVersion,
        string errorMessage)
    {
        if (currentRowVersion.Length == 0
            || expectedVersion.Length != currentRowVersion.Length
            || !CryptographicOperations.FixedTimeEquals(
                expectedVersion,
                currentRowVersion))
        {
            throw new ConflictException(errorMessage);
        }
    }
}
