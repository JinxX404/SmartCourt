using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.Email;

public class BackgroundEmailProvider : IEmailProvider
{
    private readonly IBackgroundJobProvider _jobProvider;

    public BackgroundEmailProvider(IBackgroundJobProvider jobProvider)
    {
        _jobProvider = jobProvider;
    }

    public Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        // Enqueue the email to be sent in the background by Hangfire
        _jobProvider.Enqueue<ISmtpEmailSender>(x => x.SendEmailAsync(to, subject, body, isHtml));
        
        // Return true immediately so the caller's thread isn't blocked
        return Task.FromResult(true);
    }
}
