namespace SmartCourt.Common.Exceptions;

public class PreconditionFailedException : Exception
{
    public PreconditionFailedException()
    {
    }

    public PreconditionFailedException(string message) : base(message)
    {
    }

    public PreconditionFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
