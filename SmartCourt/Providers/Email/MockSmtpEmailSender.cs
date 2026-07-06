using Microsoft.Extensions.Logging;

namespace SmartCourt.Providers.Email;

public class MockSmtpEmailSender : ISmtpEmailSender
{
    private readonly ILogger<MockSmtpEmailSender> _logger;

    public MockSmtpEmailSender(ILogger<MockSmtpEmailSender> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        _logger.LogInformation("--- MOCK EMAIL ---");
        _logger.LogInformation("To: {To}", to);
        _logger.LogInformation("Subject: {Subject}", subject);
        _logger.LogInformation("Body: {Body}", body);
        _logger.LogInformation("------------------");
        
        return Task.FromResult(true);
    }
}
