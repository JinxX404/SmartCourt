using System;

namespace SmartCourt.Common.Domain;

public static class CreditConverter
{
    private const decimal TokensPerCredit = 10000m;

    public static decimal ToCredits(int tokens)
    {
        return tokens / TokensPerCredit;
    }

    public static int ToTokens(decimal credits)
    {
        return (int)Math.Round(credits * TokensPerCredit, MidpointRounding.AwayFromZero);
    }
}
