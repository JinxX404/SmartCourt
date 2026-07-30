using System.Linq.Expressions;

namespace SmartCourt.Interfaces.Providers;

public interface IRecurringBackgroundJobProvider
{
    Task RegisterOrUpdateAsync<T>(
        string recurringJobId,
        Expression<Func<T, Task>> methodCall,
        string cronExpression,
        CancellationToken cancellationToken);
}
