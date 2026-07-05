using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using SmartCourt.Core.Interfaces.Providers;

namespace SmartCourt.Infrastructure.Providers.Email;

public interface ISmtpEmailSender
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false);
}

public class SmtpEmailSender : ISmtpEmailSender
{
    private readonly MailKitOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<MailKitOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(isHtml ? TextFormat.Html : TextFormat.Plain)
            {
                Text = body
            };

            using var smtp = new SmtpClient();
            var secureSocketOptions = _options.UseSsl ? MailKit.Security.SecureSocketOptions.Auto : MailKit.Security.SecureSocketOptions.None;
            await smtp.ConnectAsync(_options.Server, _options.Port, secureSocketOptions);
            await smtp.AuthenticateAsync(_options.Username, _options.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return false;
        }
    }
}
