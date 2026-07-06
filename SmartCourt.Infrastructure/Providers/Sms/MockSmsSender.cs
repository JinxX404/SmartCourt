using Microsoft.Extensions.Logging;

namespace SmartCourt.Infrastructure.Providers.Sms;

public class MockSmsSender : ISmsSender
{
    private readonly ILogger<MockSmsSender> _logger;

    public MockSmsSender(ILogger<MockSmsSender> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("--- MOCK SMS ---");
        _logger.LogInformation("To: {PhoneNumber}", phoneNumber);
        _logger.LogInformation("Message: {Message}", message);
        _logger.LogInformation("----------------");
        
        return Task.FromResult(true);
    }
}
