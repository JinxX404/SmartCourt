namespace SmartCourt.Interfaces.Providers;

public interface ISmsProvider
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}
