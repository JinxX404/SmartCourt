using SmartCourt.Common.RateLimiting;

namespace SmartCourt.Common.Exceptions;

public sealed class TooManyRequestsException : Exception
{
    public TooManyRequestsException()
        : base(RateLimitResponse.Message)
    {
    }
}
