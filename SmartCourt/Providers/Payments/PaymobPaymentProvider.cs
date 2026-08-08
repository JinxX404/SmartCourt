using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Infrastructure.Providers.Payments;

namespace SmartCourt.Providers.Payments;

public sealed class PaymobPaymentProvider
    : IPaymentProvider, IPaymentReconciliationProvider
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly PaymobOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymobPaymentProvider> _logger;

    public PaymobPaymentProvider(
        IOptions<PaymobOptions> options,
        HttpClient httpClient,
        ILogger<PaymobPaymentProvider> logger)
    {
        _options = options.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<ProviderResult> DepositAsync(
        ProviderDepositRequest request,
        CancellationToken cancellationToken)
        => CreateAsync(
            _options.PaymentsPath,
            new PaymobRequest(
                request.Amount,
                request.Currency,
                request.BusinessId,
                request.ProviderIdempotencyKey,
                request.CorrelationId,
                PaymentMethodReference: request.PaymentMethodReference),
            request,
            "deposit",
            cancellationToken);

    public Task<ProviderResult> RetryDepositAsync(
        ProviderDepositRetryRequest request,
        CancellationToken cancellationToken)
        => CreateAsync(
            _options.RetryPath,
            new PaymobRequest(
                request.Amount,
                request.Currency,
                request.BusinessId,
                request.ProviderIdempotencyKey,
                request.CorrelationId,
                OriginalIdempotencyKey: request.OriginalProviderIdempotencyKey,
                OriginalTransactionId: request.OriginalProviderTransactionId),
            request,
            "deposit-retry",
            cancellationToken);

    public Task<ProviderResult> ReleaseAsync(
        ProviderReleaseRequest request,
        CancellationToken cancellationToken)
        => CreateAsync(
            _options.ReleasesPath,
            new PaymobRequest(
                request.Amount,
                request.Currency,
                request.BusinessId,
                request.ProviderIdempotencyKey,
                request.CorrelationId),
            request,
            "release",
            cancellationToken);

    public Task<ProviderResult> RefundAsync(
        ProviderRefundRequest request,
        CancellationToken cancellationToken)
        => CreateAsync(
            _options.RefundsPath,
            new PaymobRequest(
                request.Amount,
                request.Currency,
                request.BusinessId,
                request.ProviderIdempotencyKey,
                request.CorrelationId,
                Reason: request.Reason),
            request,
            "refund",
            cancellationToken);

    public Task<ProviderResult> WithdrawAsync(
        ProviderWithdrawalRequest request,
        CancellationToken cancellationToken)
        => CreateAsync(
            _options.WithdrawalsPath,
            new PaymobRequest(
                request.Amount,
                request.Currency,
                request.BusinessId,
                request.ProviderIdempotencyKey,
                request.CorrelationId,
                DestinationReference: request.DestinationReference),
            request,
            "withdrawal",
            cancellationToken);

    public Task<ProviderResult?> GetDepositStatusAsync(
        ProviderDepositStatusRequest request,
        CancellationToken cancellationToken)
        => GetStatusAsync(
            _options.StatusPath,
            request.ProviderIdempotencyKey,
            request.Amount,
            request.Currency,
            request.BusinessId,
            request.CorrelationId,
            "deposit",
            cancellationToken);

    public Task<ProviderResult?> GetReleaseStatusAsync(
        ProviderReleaseStatusRequest request,
        CancellationToken cancellationToken)
        => GetStatusAsync(
            _options.StatusPath,
            request.ProviderIdempotencyKey,
            request.Amount,
            request.Currency,
            request.BusinessId,
            request.CorrelationId,
            "release",
            cancellationToken);

    public Task<ProviderResult?> GetRefundStatusAsync(
        ProviderRefundStatusRequest request,
        CancellationToken cancellationToken)
        => GetStatusAsync(
            _options.StatusPath,
            request.ProviderIdempotencyKey,
            request.Amount,
            request.Currency,
            request.BusinessId,
            request.CorrelationId,
            "refund",
            cancellationToken);

    public Task<ProviderResult?> GetWithdrawalStatusAsync(
        ProviderWithdrawalStatusRequest request,
        CancellationToken cancellationToken)
        => GetStatusAsync(
            _options.StatusPath,
            request.ProviderIdempotencyKey,
            request.Amount,
            request.Currency,
            request.BusinessId,
            request.CorrelationId,
            "withdrawal",
            cancellationToken);

    private async Task<ProviderResult?> GetStatusAsync(
        string path,
        string providerIdempotencyKey,
        decimal amount,
        string currency,
        Guid businessId,
        Guid correlationId,
        string operation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var message = new HttpRequestMessage(
            HttpMethod.Get,
            BuildEndpoint(path, providerIdempotencyKey));
        AddSecurityHeaders(message, providerIdempotencyKey);

        var response = await SendAsync(message, cancellationToken);
        var data = await ReadResponseAsync(response, cancellationToken);

        return CreateResult(
            operation,
            amount,
            currency,
            businessId,
            providerIdempotencyKey,
            correlationId,
            response.IsSuccessStatusCode,
            data);
    }

    private async Task<ProviderResult> CreateAsync(
        string path,
        PaymobRequest payload,
        PaymentProviderRequest request,
        string operation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            BuildEndpoint(path))
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };
        AddSecurityHeaders(message, request.ProviderIdempotencyKey);

        var response = await SendAsync(message, cancellationToken);
        var data = await ReadResponseAsync(response, cancellationToken);

        return CreateResult(
            operation,
            request.Amount,
            request.Currency,
            request.BusinessId,
            request.ProviderIdempotencyKey,
            request.CorrelationId,
            response.IsSuccessStatusCode,
            data);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.SendAsync(message, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "Paymob request failed before a response was received.");

            return new HttpResponseMessage(
                System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"status\":\"pending\",\"message\":\"transport-error\"}")
            };
        }
    }

    private static async Task<PaymobResponse?> ReadResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<PaymobResponse>(
                JsonOptions,
                cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ProviderResult CreateResult(
        string operation,
        decimal amount,
        string currency,
        Guid businessId,
        string providerIdempotencyKey,
        Guid correlationId,
        bool isSuccess,
        PaymobResponse? data)
    {
        var outcome = !isSuccess
            ? ProviderOperationOutcome.Failed
            : MapStatus(data?.Status);

        var providerTransactionId = outcome == ProviderOperationOutcome.Succeeded
            ? data?.Id ?? $"paymob-{providerIdempotencyKey}"
            : null;

        var failureReason = outcome switch
        {
            ProviderOperationOutcome.Failed =>
                data?.Message ?? "Paymob returned a failed outcome.",
            ProviderOperationOutcome.Unknown =>
                "Paymob outcome is pending or unknown and requires reconciliation.",
            _ => null
        };

        return new ProviderResult(
            amount,
            currency,
            businessId,
            providerIdempotencyKey,
            correlationId,
            outcome,
            providerTransactionId,
            failureReason);
    }

    private void AddSecurityHeaders(
        HttpRequestMessage message,
        string idempotencyKey)
    {
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            message.Headers.Add("X-Api-Key", _options.ApiKey);
        }

        message.Headers.Add("Idempotency-Key", idempotencyKey);
    }

    private Uri BuildEndpoint(string path, string idempotencyKey = "")
    {
        var baseUri = new Uri(_options.BaseUrl.TrimEnd('/'), UriKind.Absolute);
        var relative = idempotencyKey.Length == 0
            ? path.TrimStart('/')
            : $"{path.Trim('/')}/{Uri.EscapeDataString(idempotencyKey)}";
        return new Uri(baseUri, relative);
    }

    public static ProviderOperationOutcome MapStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return ProviderOperationOutcome.Unknown;
        }

        return status.Trim().ToLowerInvariant() switch
        {
            "succeeded" or "approved" or "captured" or "completed" or "paid" =>
                ProviderOperationOutcome.Succeeded,
            "failed" or "declined" or "refused" or "rejected" or "expired" =>
                ProviderOperationOutcome.Failed,
            _ => ProviderOperationOutcome.Unknown
        };
    }

    public static bool IsValidWebhookSignature(
        string webhookSecret,
        string payload,
        string providedSignature)
    {
        if (string.IsNullOrEmpty(webhookSecret)
            || string.IsNullOrWhiteSpace(providedSignature))
        {
            return false;
        }

        var computed = Convert.ToHexString(
            HashWebhookPayload(webhookSecret, payload));
        var expected = Encoding.ASCII.GetBytes(computed);
        var actual = Encoding.ASCII.GetBytes(providedSignature);

        return expected.Length == actual.Length
            && CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    private static byte[] HashWebhookPayload(
        string webhookSecret,
        string payload)
    {
        using var hmac = new HMACSHA256(
            Encoding.UTF8.GetBytes(webhookSecret));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
    }

    internal sealed record PaymobRequest(
        decimal Amount,
        string Currency,
        Guid BusinessId,
        string IdempotencyKey,
        Guid CorrelationId,
        string? PaymentMethodReference = null,
        string? OriginalIdempotencyKey = null,
        string? OriginalTransactionId = null,
        string? Reason = null,
        string? DestinationReference = null);

    internal sealed record PaymobResponse(
        string? Id,
        string? Status,
        string? Message);
}
