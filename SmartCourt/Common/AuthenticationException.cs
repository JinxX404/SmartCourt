using System;

namespace SmartCourt.Common;

public class AuthenticationException : Exception
{
    public AuthenticationException(string message)
        : base(message)
    {
    }
}
