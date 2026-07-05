namespace SmartCourt.Core.Interfaces.Providers;

public interface IEmailProvider
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false);
}
