using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SmartCourt.Providers.Sms;

public interface ISmsSender
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}

public class TwilioSmsSender : ISmsSender
{
    private readonly TwilioOptions _options;
    private readonly ILogger<TwilioSmsSender> _logger;

    public TwilioSmsSender(IOptions<TwilioOptions> options, ILogger<TwilioSmsSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            TwilioClient.Init(_options.AccountSid, _options.AuthToken);

            var messageResource = await MessageResource.CreateAsync(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(_options.FromNumber),
                body: message
            );

            _logger.LogInformation("SMS Sent successfully via Twilio. SID: {Sid}", messageResource.Sid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber} via Twilio.", phoneNumber);
            return false;
        }
    }
}
