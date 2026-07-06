using SmartCourt.Core.Interfaces.Providers;

namespace SmartCourt.Infrastructure.Providers.Sms;

public class BackgroundSmsProvider : ISmsProvider
{
    private readonly IBackgroundJobProvider _jobProvider;

    public BackgroundSmsProvider(IBackgroundJobProvider jobProvider)
    {
        _jobProvider = jobProvider;
    }

    public Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        _jobProvider.Enqueue<ISmsSender>(x => x.SendSmsAsync(phoneNumber, message));
        
        return Task.FromResult(true);
    }
}
