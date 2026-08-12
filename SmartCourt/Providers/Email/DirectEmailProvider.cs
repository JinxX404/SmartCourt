using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.Email;

public class DirectEmailProvider : IEmailProvider
{
    private readonly ISmtpEmailSender _sender;

    public DirectEmailProvider(ISmtpEmailSender sender)
    {
        _sender = sender;
    }

    public Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default)
    {
        return _sender.SendEmailAsync(to, subject, body, isHtml);
    }
}
