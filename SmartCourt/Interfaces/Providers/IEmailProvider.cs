namespace SmartCourt.Interfaces.Providers;

public interface IEmailProvider
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default);
}
