using System.Linq.Expressions;
using Hangfire;
using Hangfire.Common;
using SmartCourt.Common.Exceptions;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.Jobs;

public sealed class HangfireRecurringBackgroundJobProvider(
    IRecurringJobManager recurringJobManager)
    : IRecurringBackgroundJobProvider
{
    public Task RegisterOrUpdateAsync<T>(
        string recurringJobId,
        Expression<Func<T, Task>> methodCall,
        string cronExpression,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(recurringJobId)
            || methodCall is null
            || string.IsNullOrWhiteSpace(cronExpression))
        {
            throw new BusinessException(
                "بيانات المهمة الخلفية المجدولة غير مكتملة.");
        }

        recurringJobManager.AddOrUpdate(
            recurringJobId,
            Job.FromExpression(methodCall),
            cronExpression,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        return Task.CompletedTask;
    }
}
