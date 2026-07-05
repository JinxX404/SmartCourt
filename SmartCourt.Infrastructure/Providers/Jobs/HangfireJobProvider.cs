using System.Linq.Expressions;
using Hangfire;
using SmartCourt.Core.Interfaces.Providers;

namespace SmartCourt.Infrastructure.Providers.Jobs;

public class HangfireJobProvider : IBackgroundJobProvider
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireJobProvider(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public string Enqueue(Expression<Action> methodCall)
    {
        return _backgroundJobClient.Enqueue(methodCall);
    }

    public string Enqueue<T>(Expression<Action<T>> methodCall)
    {
        return _backgroundJobClient.Enqueue<T>(methodCall);
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        return _backgroundJobClient.Enqueue<T>(methodCall);
    }
}
