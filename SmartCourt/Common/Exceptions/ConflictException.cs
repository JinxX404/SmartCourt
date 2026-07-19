using System;

namespace SmartCourt.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
