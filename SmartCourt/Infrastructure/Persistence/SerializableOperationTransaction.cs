using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmartCourt.Persistence;

namespace SmartCourt.Infrastructure.Persistence;

internal sealed class SerializableOperationTransaction
    : IAsyncDisposable
{
    private readonly ApplicationDbContext _dbContext;

    private SerializableOperationTransaction(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    internal IDbContextTransaction? Current { get; private set; }

    internal static async Task<SerializableOperationTransaction>
        CreateAsync(
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken)
    {
        var scope = new SerializableOperationTransaction(dbContext);
        await scope.BeginAsync(cancellationToken);
        return scope;
    }

    internal async Task BeginAsync(
        CancellationToken cancellationToken)
    {
        if (Current is not null)
        {
            throw new InvalidOperationException(
                "A serializable operation transaction is already active.");
        }

        if (_dbContext.Database.IsRelational())
        {
            Current = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
        }
    }

    internal async Task CommitAndCloseAsync(
        CancellationToken cancellationToken)
    {
        if (Current is null)
        {
            return;
        }

        await Current.CommitAsync(cancellationToken);
        await Current.DisposeAsync();
        Current = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (Current is null)
        {
            return;
        }

        await Current.DisposeAsync();
        Current = null;
    }
}
