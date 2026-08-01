using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Infrastructure.Providers.Payments;

namespace SmartCourt.Providers.Payments;

public sealed class MockPaymentProvider
    : IPaymentProvider, IPaymentReconciliationProvider
{
    private readonly ConcurrentDictionary<string, ProviderResult> _results = new();
    private readonly ILogger<MockPaymentProvider> _logger;

    public MockPaymentProvider(
        IOptions<PaymentProviderOptions> options,
        ILogger<MockPaymentProvider> logger)
    {
        _logger = logger;

        if (!options.Value.UseMockProvider)
        {
            _logger.LogWarning(
                "Mock payment provider was constructed while configuration marks it disabled.");
        }
    }

    public async Task<ProviderResult> DepositAsync(
        ProviderDepositRequest request,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(
            "deposit",
            request,
            request.PaymentMethodReference,
            cancellationToken);
    }

    public async Task<ProviderResult> RetryDepositAsync(
        ProviderDepositRetryRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_results.TryGetValue(
                $"deposit:{request.OriginalProviderIdempotencyKey}",
                out var originalResult))
        {
            return await ExecuteAsync(
                "deposit-retry",
                request,
                originalResult.Outcome switch
                {
                    ProviderOperationOutcome.Succeeded =>
                        "mock-success-retry",
                    ProviderOperationOutcome.Failed =>
                        "mock-fail-retry",
                    _ => "mock-timeout-retry"
                },
                cancellationToken);
        }

        return await ExecuteAsync(
            "deposit-retry",
            request,
            "mock-timeout-retry",
            cancellationToken);
    }

    public async Task<ProviderResult> ReleaseAsync(
        ProviderReleaseRequest request,
        CancellationToken cancellationToken)
    {
        var behaviorReference = request.ProviderIdempotencyKey
            .StartsWith("mock-", StringComparison.OrdinalIgnoreCase)
            ? request.ProviderIdempotencyKey
            : "mock-success-release";
        return await ExecuteAsync(
            "release",
            request,
            behaviorReference,
            cancellationToken);
    }

    public async Task<ProviderResult> RefundAsync(
        ProviderRefundRequest request,
        CancellationToken cancellationToken)
    {
        var behaviorReference = request.ProviderIdempotencyKey
            .StartsWith("mock-", StringComparison.OrdinalIgnoreCase)
            ? request.ProviderIdempotencyKey
            : "mock-success-refund";
        return await ExecuteAsync(
            "refund",
            request,
            behaviorReference,
            cancellationToken);
    }

    public async Task<ProviderResult> WithdrawAsync(
        ProviderWithdrawalRequest request,
        CancellationToken cancellationToken)
    {
        var behaviorReference = request.ProviderIdempotencyKey
            .StartsWith("mock-", StringComparison.OrdinalIgnoreCase)
            ? request.ProviderIdempotencyKey
            : "mock-success-withdrawal";
        return await ExecuteAsync(
            "withdrawal",
            request,
            behaviorReference,
            cancellationToken);
    }

    public async Task<ProviderResult?> GetDepositStatusAsync(
        ProviderDepositStatusRequest request,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();
        _results.TryGetValue(
            $"deposit:{request.ProviderIdempotencyKey}",
            out var result);
        return result;
    }

    public async Task<ProviderResult?> GetReleaseStatusAsync(
        ProviderReleaseStatusRequest request,
        CancellationToken cancellationToken)
        => await GetStatusAsync(
            "release",
            request.ProviderIdempotencyKey,
            cancellationToken);

    public async Task<ProviderResult?> GetRefundStatusAsync(
        ProviderRefundStatusRequest request,
        CancellationToken cancellationToken)
        => await GetStatusAsync(
            "refund",
            request.ProviderIdempotencyKey,
            cancellationToken);

    public async Task<ProviderResult?> GetWithdrawalStatusAsync(
        ProviderWithdrawalStatusRequest request,
        CancellationToken cancellationToken)
        => await GetStatusAsync(
            "withdrawal",
            request.ProviderIdempotencyKey,
            cancellationToken);

    private async Task<ProviderResult?> GetStatusAsync(
        string operation,
        string providerIdempotencyKey,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();
        _results.TryGetValue(
            $"{operation}:{providerIdempotencyKey}",
            out var result);
        return result;
    }

    private async Task<ProviderResult> ExecuteAsync(
        string operation,
        PaymentProviderRequest request,
        string behaviorReference,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();

        var cacheKey = $"{operation}:{request.ProviderIdempotencyKey}";
        var result = _results.GetOrAdd(
            cacheKey,
            _ => CreateResult(operation, request, behaviorReference));

        _logger.LogInformation(
            "Mock payment {Operation} for business {BusinessId} completed with {Outcome}. CorrelationId: {CorrelationId}",
            operation,
            request.BusinessId,
            result.Outcome,
            request.CorrelationId);

        return result;
    }

    private static ProviderResult CreateResult(
        string operation,
        PaymentProviderRequest request,
        string behaviorReference)
    {
        var outcome = ResolveOutcome(behaviorReference);
        var providerTransactionId = outcome == ProviderOperationOutcome.Succeeded
            ? $"mock-{operation}-{CreateDeterministicId(operation, request.ProviderIdempotencyKey):N}"
            : null;
        var failureReason = outcome switch
        {
            ProviderOperationOutcome.Failed =>
                "Mock provider confirmed failure.",
            ProviderOperationOutcome.Unknown =>
                "Mock provider outcome is unknown and requires reconciliation.",
            _ => null
        };

        return new ProviderResult(
            request.Amount,
            request.Currency,
            request.BusinessId,
            request.ProviderIdempotencyKey,
            request.CorrelationId,
            outcome,
            providerTransactionId,
            failureReason);
    }

    private static ProviderOperationOutcome ResolveOutcome(string behaviorReference)
    {
        if (behaviorReference.StartsWith(
                "mock-success",
                StringComparison.OrdinalIgnoreCase))
        {
            return ProviderOperationOutcome.Succeeded;
        }

        if (behaviorReference.StartsWith(
                "mock-fail",
                StringComparison.OrdinalIgnoreCase))
        {
            return ProviderOperationOutcome.Failed;
        }

        if (behaviorReference.StartsWith(
                "mock-timeout",
                StringComparison.OrdinalIgnoreCase))
        {
            return ProviderOperationOutcome.Unknown;
        }

        return ProviderOperationOutcome.Failed;
    }

    private static Guid CreateDeterministicId(
        string operation,
        string providerIdempotencyKey)
    {
        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes($"{operation}:{providerIdempotencyKey}"));
        return new Guid(bytes[..16]);
    }
}
