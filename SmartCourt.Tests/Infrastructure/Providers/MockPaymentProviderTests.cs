using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Providers.Payments;
using Xunit;

namespace SmartCourt.Tests.Infrastructure.Providers;

public sealed class MockPaymentProviderTests
{
    private static readonly Guid BusinessId =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private static readonly Guid CorrelationId =
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task Deposit_UsesPaymentReferenceToReturnSuccessFailureOrUnknown()
    {
        var provider = CreateProvider();

        var success = await provider.DepositAsync(
            DepositRequest("deposit-success", "mock-success-card"),
            CancellationToken.None);
        var failure = await provider.DepositAsync(
            DepositRequest("deposit-fail", "mock-fail-card"),
            CancellationToken.None);
        var unknown = await provider.DepositAsync(
            DepositRequest("deposit-timeout", "mock-timeout-card"),
            CancellationToken.None);

        Assert.Equal(ProviderOperationOutcome.Succeeded, success.Outcome);
        Assert.NotNull(success.ProviderTransactionId);
        Assert.Equal(ProviderOperationOutcome.Failed, failure.Outcome);
        Assert.Null(failure.ProviderTransactionId);
        Assert.Equal(ProviderOperationOutcome.Unknown, unknown.Outcome);
        Assert.Contains("unknown", unknown.FailureReason);
    }

    [Theory]
    [MemberData(nameof(OperationRequests))]
    public async Task EveryOperation_UsesIdempotencyKeyForDeterministicOutcome(
        string operation,
        PaymentProviderRequest request,
        ProviderOperationOutcome expectedOutcome)
    {
        var provider = CreateProvider();

        var first = await ExecuteAsync(provider, operation, request);
        var second = await ExecuteAsync(provider, operation, request);

        Assert.Equal(expectedOutcome, first.Outcome);
        Assert.Equal(first, second);
    }

    [Fact]
    public async Task Cancellation_IsHonoredBeforeProviderResult()
    {
        var provider = CreateProvider();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => provider.DepositAsync(
                DepositRequest("cancelled", "mock-success-card"),
                cancellation.Token));
    }

    [Fact]
    public async Task DuplicateKey_ReturnsOriginalResultEvenWhenRequestPayloadChanges()
    {
        var provider = CreateProvider();
        var firstRequest = DepositRequest(
            "same-key",
            "mock-success-card",
            amount: 100m);
        var changedRequest = DepositRequest(
            "same-key",
            "mock-fail-card",
            amount: 999m);

        var first = await provider.DepositAsync(
            firstRequest,
            CancellationToken.None);
        var replay = await provider.DepositAsync(
            changedRequest,
            CancellationToken.None);

        Assert.Equal(first, replay);
    }

    [Fact]
    public void Options_DefaultWarningIdentifiesTheProviderAsUnregulated()
    {
        var options = new PaymentProviderOptions();

        Assert.False(options.UseMockProvider);
        Assert.Contains("not regulated escrow", options.Warning);
    }

    public static IEnumerable<object[]> OperationRequests()
    {
        yield return
        [
            "deposit",
            DepositRequest("deposit-success", "mock-success-card"),
            ProviderOperationOutcome.Succeeded
        ];
        yield return
        [
            "release",
            new ProviderReleaseRequest(
                100m,
                "EGP",
                BusinessId,
                "mock-fail-release",
                CorrelationId),
            ProviderOperationOutcome.Failed
        ];
        yield return
        [
            "refund",
            new ProviderRefundRequest(
                100m,
                "EGP",
                BusinessId,
                "mock-timeout-refund",
                CorrelationId,
                "test"),
            ProviderOperationOutcome.Unknown
        ];
        yield return
        [
            "withdrawal",
            new ProviderWithdrawalRequest(
                100m,
                "EGP",
                BusinessId,
                "mock-success-withdrawal",
                CorrelationId),
            ProviderOperationOutcome.Succeeded
        ];
    }

    private static ProviderDepositRequest DepositRequest(
        string idempotencyKey,
        string paymentMethodReference,
        decimal amount = 100m)
    {
        return new ProviderDepositRequest(
            amount,
            "EGP",
            BusinessId,
            idempotencyKey,
            CorrelationId,
            paymentMethodReference);
    }

    private static MockPaymentProvider CreateProvider()
    {
        return new MockPaymentProvider(
            Options.Create(new PaymentProviderOptions
            {
                UseMockProvider = true
            }),
            NullLogger<MockPaymentProvider>.Instance);
    }

    private static Task<ProviderResult> ExecuteAsync(
        MockPaymentProvider provider,
        string operation,
        PaymentProviderRequest request)
    {
        return operation switch
        {
            "deposit" => provider.DepositAsync(
                (ProviderDepositRequest)request,
                CancellationToken.None),
            "release" => provider.ReleaseAsync(
                (ProviderReleaseRequest)request,
                CancellationToken.None),
            "refund" => provider.RefundAsync(
                (ProviderRefundRequest)request,
                CancellationToken.None),
            "withdrawal" => provider.WithdrawAsync(
                (ProviderWithdrawalRequest)request,
                CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
    }
}
