using System.Linq.Expressions;
using SmartCourt.Features.Proposals.Expiration;
using SmartCourt.Interfaces.Providers;
using Xunit;

namespace SmartCourt.Tests.Infrastructure.Providers;

public sealed class ProposalRecurringJobRegistrarTests
{
    [Fact]
    public async Task RegisterAsync_RegistersMinuteExpirationJob()
    {
        var provider = new RecordingRecurringJobProvider();
        var registrar = new ProposalRecurringJobRegistrar(provider);

        await registrar.RegisterAsync(CancellationToken.None);

        Assert.Equal("proposal-expiration", provider.JobId);
        Assert.Equal("*/1 * * * *", provider.CronExpression);
        Assert.Equal(
            nameof(IProposalExpirationService.ExpireDueAsync),
            provider.MethodName);
    }

    private sealed class RecordingRecurringJobProvider
        : IRecurringBackgroundJobProvider
    {
        public string? JobId { get; private set; }
        public string? CronExpression { get; private set; }
        public string? MethodName { get; private set; }

        public Task RegisterOrUpdateAsync<T>(
            string recurringJobId,
            Expression<Func<T, Task>> methodCall,
            string cronExpression,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            JobId = recurringJobId;
            CronExpression = cronExpression;
            MethodName = ((MethodCallExpression)methodCall.Body).Method.Name;
            return Task.CompletedTask;
        }
    }
}
