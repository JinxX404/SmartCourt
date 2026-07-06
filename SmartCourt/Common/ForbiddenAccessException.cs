using System;

namespace SmartCourt.Common;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base()
    {
    }

    public ForbiddenAccessException(string message) : base(message)
    {
    }
}
