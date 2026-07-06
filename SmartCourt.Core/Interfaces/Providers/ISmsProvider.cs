namespace SmartCourt.Core.Interfaces.Providers;

public interface ISmsProvider
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}
