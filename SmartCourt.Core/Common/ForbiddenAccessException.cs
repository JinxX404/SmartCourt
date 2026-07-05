using System;

namespace SmartCourt.Core.Common;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base()
    {
    }

    public ForbiddenAccessException(string message) : base(message)
    {
    }
}
