using System.Linq.Expressions;
using Hangfire;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.Jobs;

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

    public async Task<string> EnqueueAsync<T>(
        Expression<Func<T, Task>> methodCall,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var jobId = _backgroundJobClient.Enqueue(methodCall);
        return await Task.FromResult(jobId);
    }

    public async Task<string> ScheduleAsync<T>(
        Expression<Func<T, Task>> methodCall,
        DateTimeOffset runAt,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var jobId = _backgroundJobClient.Schedule(methodCall, runAt);
        return await Task.FromResult(jobId);
    }
}
