using Microsoft.Extensions.Options;
using SmartCourt.Common.Configuration;

namespace SmartCourt.Infrastructure.Providers.Events;

public sealed class OutboxDispatchBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxDispatchOptions> options,
    ILogger<OutboxDispatchBackgroundService> logger)
    : BackgroundService
{
    private readonly OutboxDispatchOptions _options = options.Value;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("The near-real-time outbox pump is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var dispatcher = scope.ServiceProvider
                    .GetRequiredService<IOutboxDispatcher>();
                var processed = await dispatcher.DispatchAvailableAsync(
                    _options.BatchSize,
                    stoppingToken);
                if (processed < _options.BatchSize)
                {
                    await Task.Delay(
                        _options.IdleDelayMilliseconds,
                        stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Near-real-time outbox dispatch failed; retrying after the configured error delay.");
                await Task.Delay(
                    _options.ErrorDelayMilliseconds,
                    stoppingToken);
            }
        }
    }
}
