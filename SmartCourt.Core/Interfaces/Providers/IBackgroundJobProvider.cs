using System.Linq.Expressions;

namespace SmartCourt.Core.Interfaces.Providers;

public interface IBackgroundJobProvider
{
    string Enqueue(Expression<Action> methodCall);
    string Enqueue<T>(Expression<Action<T>> methodCall);
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);
}
