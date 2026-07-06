using System;

namespace SmartCourt.Common;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {

    }

    public BusinessException(string message, Exception innerException) : base(message, innerException)
    {
        
    }
}
