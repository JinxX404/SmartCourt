using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Providers.Payments;
using Xunit;

namespace SmartCourt.Tests.Infrastructure.Providers;

public sealed class PaymobPaymentProviderTests
{
    private static readonly Guid BusinessId =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private static readonly Guid CorrelationId =
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static readonly PaymobOptions DefaultOptions = new()
    {
        BaseUrl = "https://api.paymob.test/v1",
        ApiKey = "test-api-key",
        WebhookSecret = "test-webhook-secret"
    };

    [Fact]
    public async Task Deposit_PostsPayloadAndMapsSucceededOutcome()
    {
        var captured = new CapturedRequest();
        var provider = CreateProvider(
            (request, response) =>
            {
                captured.Method = request.Method;
                captured.Path = request.RequestUri!.AbsolutePath;
                captured.IdempotencyKey = request.Headers
                    .GetValues("Idempotency-Key")
                    .Single();
                captured.ApiKey = request.Headers
                    .TryGetValues("X-Api-Key", out var values)
                    ? values.Single()
                    : null;
                captured.Body = request.Content!.ReadAsStringAsync().GetAwaiter()
                    .GetResult();
                ReturnJson(
                    response,
                    """{"id":"pmb_deposit_1","status":"succeeded"}""");
            });

        var result = await provider.DepositAsync(
            DepositRequest("deposit-key-1", "card_token_123"),
            CancellationToken.None);

        Assert.Equal(HttpMethod.Post, captured.Method);
        Assert.Equal("/payments", captured.Path);
        Assert.Equal("deposit-key-1", captured.IdempotencyKey);
        Assert.Equal("test-api-key", captured.ApiKey);
        Assert.Equal(ProviderOperationOutcome.Succeeded, result.Outcome);
        Assert.Equal("pmb_deposit_1", result.ProviderTransactionId);

        var body = JsonDocument.Parse(captured.Body!).RootElement;
        Assert.Equal(100m, body.GetProperty("amount").GetDecimal());
        Assert.Equal(
            "card_token_123",
            body.GetProperty("paymentMethodReference").GetString());
        Assert.Equal(
            "deposit-key-1",
            body.GetProperty("idempotencyKey").GetString());
    }

    [Fact]
    public async Task Deposit_FailedStatusMapsToFailedOutcome()
    {
        var provider = CreateProvider(ReturnJson(
            """{"id":"pm_deposit_1","status":"declined","message":"insufficient funds"}"""));

        var result = await provider.DepositAsync(
            DepositRequest("deposit-key", "mock-fail-card"),
            CancellationToken.None);

        Assert.Equal(ProviderOperationOutcome.Failed, result.Outcome);
        Assert.Null(result.ProviderTransactionId);
        Assert.Contains("insufficient funds", result.FailureReason);
    }

    [Fact]
    public async Task Deposit_PendingStatusMapsToUnknownOutcome()
    {
        var provider = CreateProvider(ReturnJson(
            """{"id":"pm_deposit_1","status":"pending","message":"awaiting settlement"}"""));

        var result = await provider.DepositAsync(
            DepositRequest("deposit-key-1", "mock-timeout-card"),
            CancellationToken.None);

        Assert.Equal(ProviderOperationOutcome.Unknown, result.Outcome);
        Assert.Contains("reconciliation", result.FailureReason);
    }

    [Fact]
    public async Task RetryDeposit_PreservesOriginalTransactionDetails()
    {
        var captured = new CapturedRequest();
        var provider = CreateProvider(
            (request, response) =>
            {
                captured.Path = request.RequestUri!.AbsolutePath;
                captured.Body = request.Content!.ReadAsStringAsync().GetAwaiter()
                    .GetResult();
                ReturnJson(response, """{"id":"pm_retry_1","status":"succeeded"}""");
            });

        var request = new ProviderDepositRetryRequest(
            750m,
            "EGP",
            BusinessId,
            "retry-key",
            CorrelationId,
            "original-key",
            "pm_original_1");

        var result = await provider.RetryDepositAsync(
            request,
            CancellationToken.None);

        Assert.Equal("/payments/retry", captured.Path);
        Assert.Equal(ProviderOperationOutcome.Succeeded, result.Outcome);
        Assert.Equal("pm_retry_1", result.ProviderTransactionId);

        var body = JsonDocument.Parse(captured.Body!).RootElement;
        Assert.Equal(
            "original-key",
            body.GetProperty("originalIdempotencyKey").GetString());
        Assert.Equal(
            "pm_original_1",
            body.GetProperty("originalTransactionId").GetString());
    }

    [Fact]
    public async Task Release_UsesReleasesEndpointAndReturnsTransactionId()
    {
        var captured = new CapturedRequest();
        var provider = CreateProvider(
            (request, response) =>
            {
                captured.Path = request.RequestUri!.AbsolutePath;
                ReturnJson(response, """{"id":"pm_release_1","status":"succeeded"}""");
            });

        var result = await provider.ReleaseAsync(
            new ProviderReleaseRequest(
                999m,
                "EGP",
                BusinessId,
                $"release-{Guid.NewGuid():N}",
                CorrelationId),
            CancellationToken.None);

        Assert.Equal("/payouts/releases", captured.Path);
        Assert.Equal(ProviderOperationOutcome.Succeeded, result.Outcome);
        Assert.Equal("pm_release_1", result.ProviderTransactionId);
    }

    [Fact]
    public async Task Refund_CarriesReasonInPayload()
    {
        var captured = new CapturedRequest();
        var provider = CreateProvider(
            (request, response) =>
            {
                captured.Path = request.RequestUri!.AbsolutePath;
                captured.Body = request.Content!.ReadAsStringAsync().GetAwaiter()
                    .GetResult();
                ReturnJson(response, """{"id":"pm_refund_1","status":"succeeded"}""");
            });

        var result = await provider.RefundAsync(
            new ProviderRefundRequest(
                50m,
                "EGP",
                BusinessId,
                $"refund-{Guid.NewGuid():N}",
                CorrelationId,
                "إنهاء العقد."),
            CancellationToken.None);

        Assert.Equal("/refunds", captured.Path);
        Assert.Equal(ProviderOperationOutcome.Succeeded, result.Outcome);

        var body = JsonDocument.Parse(captured.Body!).RootElement;
        Assert.Equal(
            "إنهاء العقد.",
            body.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task Withdrawal_CarriesDestinationReferenceInPayload()
    {
        var captured = new CapturedRequest();
        var provider = CreateProvider(
            (request, response) =>
            {
                captured.Path = request.RequestUri!.AbsolutePath;
                captured.Body = request.Content!.ReadAsStringAsync().GetAwaiter()
                    .GetResult();
                ReturnJson(response, """{"id":"pm_with_1","status":"succeeded"}""");
            });

        var result = await provider.WithdrawAsync(
            new ProviderWithdrawalRequest(
                250m,
                "EGP",
                BusinessId,
                $"withdrawal-{Guid.NewGuid():N}",
                CorrelationId,
                "bank-account-token"),
            CancellationToken.None);

        Assert.Equal("/payouts/withdrawals", captured.Path);
        Assert.Equal(ProviderOperationOutcome.Succeeded, result.Outcome);

        var body = JsonDocument.Parse(captured.Body!).RootElement;
        Assert.Equal(
            "bank-account-token",
            body.GetProperty("destinationReference").GetString());
    }

    [Fact]
    public async Task NonSuccessHttpResponse_ReturnsFailedOutcome()
    {
        var provider = CreateProvider(
            (request, response) =>
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Content = new StringContent(
                    """{"status":"failed","message":"bad request"}""");
            });

        var result = await provider.DepositAsync(
            DepositRequest("deposit-key", "card"),
            CancellationToken.None);

        Assert.Equal(ProviderOperationOutcome.Failed, result.Outcome);
        Assert.Contains("bad request", result.FailureReason);
    }

    [Fact]
    public async Task TransportFailure_ReturnsUnknownForReconciliation()
    {
        var provider = CreateProvider(
            (_, _) => throw new HttpRequestException("connection refused"));

        var result = await provider.DepositAsync(
            DepositRequest("deposit-key", "card"),
            CancellationToken.None);

        Assert.Equal(ProviderOperationOutcome.Unknown, result.Outcome);
        Assert.Contains("pending", result.FailureReason);
    }

    [Fact]
    public async Task GetStatus_QueriesOperationEndpointWithKey()
    {
        var provider = CreateProvider(ReturnJson(
            """{"id":"pm_deposit_1","status":"paid"}"""));

        var status = await provider.GetDepositStatusAsync(
            new ProviderDepositStatusRequest(
                100m,
                "EGP",
                BusinessId,
                "deposit-key-1",
                CorrelationId),
            CancellationToken.None);

        Assert.NotNull(status);
        Assert.Equal(ProviderOperationOutcome.Succeeded, status!.Outcome);
        Assert.Equal("pm_deposit_1", status.ProviderTransactionId);
    }

    [Fact]
    public async Task GetStatus_SetsFailedOutcomeOnHttpError()
    {
        var provider = CreateProvider(
            (request, response) =>
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Content = new StringContent("""{"message":"not found"}""");
            });

        var status = await provider.GetDepositStatusAsync(
            new ProviderDepositStatusRequest(
                100m,
                "EGP",
                BusinessId,
                "missing-key",
                CorrelationId),
            CancellationToken.None);

        Assert.NotNull(status);
        Assert.Equal(ProviderOperationOutcome.Failed, status!.Outcome);
    }

    [Theory]
    [InlineData("succeeded", ProviderOperationOutcome.Succeeded)]
    [InlineData("approved", ProviderOperationOutcome.Succeeded)]
    [InlineData("paid", ProviderOperationOutcome.Succeeded)]
    [InlineData("failed", ProviderOperationOutcome.Failed)]
    [InlineData("declined", ProviderOperationOutcome.Failed)]
    [InlineData("pending", ProviderOperationOutcome.Unknown)]
    [InlineData("", ProviderOperationOutcome.Unknown)]
    [InlineData(null, ProviderOperationOutcome.Unknown)]
    public void MapStatus_AcceptsPaymobStatusVocabulary(
        string? status,
        ProviderOperationOutcome expected)
    {
        Assert.Equal(
            expected,
            PaymobPaymentProvider.MapStatus(status));
    }

    [Fact]
    public void IsValidWebhookSignature_ValidatesHmacHeader()
    {
        const string payload = """{"event":"deposit.succeeded","id":"pm_1"}""";

        var signature = ComputeSignature(payload);

        Assert.True(PaymobPaymentProvider.IsValidWebhookSignature(
            "test-webhook-secret",
            payload,
            signature));
        Assert.False(PaymobPaymentProvider.IsValidWebhookSignature(
            "test-webhook-secret",
            payload,
            "wrong-signature"));
        Assert.False(PaymobPaymentProvider.IsValidWebhookSignature(
            "test-webhook-secret",
            """{"event":"tampered"}""",
            signature));
        Assert.False(PaymobPaymentProvider.IsValidWebhookSignature(
            "test-webhook-secret",
            payload,
            ""));
    }

    private static PaymobPaymentProvider CreateProvider(
        Action<HttpRequestMessage, HttpResponseMessage> handler)
    {
        var handlerStub = new StubHttpMessageHandler(handler);
        var httpClient = new HttpClient(handlerStub);

        return new PaymobPaymentProvider(
            Options.Create(DefaultOptions),
            httpClient,
            NullLogger<PaymobPaymentProvider>.Instance);
    }

    private static Action<HttpRequestMessage, HttpResponseMessage> ReturnJson(
        string json)
        => (_, response) =>
        {
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent(json);
        };

    private static void ReturnJson(
        HttpResponseMessage response,
        string json)
    {
        response.StatusCode = HttpStatusCode.OK;
        response.Content = new StringContent(json);
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

    private static string ComputeSignature(string payload)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            System.Text.Encoding.UTF8.GetBytes("test-webhook-secret"));
        return Convert.ToHexString(
            hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload)));
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Action<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHttpMessageHandler(
            Action<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage();
            _handler(request, response);
            return Task.FromResult(response);
        }
    }

    private sealed class CapturedRequest
    {
        public HttpMethod? Method { get; set; }

        public string? Path { get; set; }

        public string? IdempotencyKey { get; set; }

        public string? ApiKey { get; set; }

        public string? Body { get; set; }
    }
}