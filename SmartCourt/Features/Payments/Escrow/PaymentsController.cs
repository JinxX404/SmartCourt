using System.Net;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Common.Validation;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Providers.Payments;

namespace SmartCourt.Features.Payments;

[ApiController]
[Route("api")]
[Authorize]
public sealed class PaymentsController(
    IPaymentEscrowService paymentEscrowService,
    IPaymentQueryService paymentQueryService,
    IPaymentWebhookService paymentWebhookService,
    IValidator<RetryPaymentRequest> retryValidator,
    IValidator<CreateMilestonePaymentSessionRequest> paymentSessionValidator,
    IValidator<RetryPaymentSessionRequest> retrySessionValidator,
    IValidator<PaymentWebhookRequest> webhookValidator,
    IOptions<PaymentProviderOptions> paymentProviderOptions)
    : ControllerBase
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    [HttpPost("milestones/{milestoneId:guid}/fund")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialMutation)]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ApiResponse<FundingOperationDto>>> FundAsync(
        Guid milestoneId,
        [FromBody] FundMilestoneRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var payment = await paymentEscrowService.FundAsync(
            milestoneId,
            request,
            idempotencyKey,
            cancellationToken);
        return payment.Payment is null
            ? Accepted(ApiResponse<FundingOperationDto>.Ok(payment))
            : Ok(ApiResponse<FundingOperationDto>.Ok(payment));
    }

    [HttpPost("milestones/{milestoneId:guid}/payment-session")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialMutation)]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ApiResponse<FundingOperationDto>>>
        CreatePaymentSessionAsync(
            Guid milestoneId,
            [FromBody] CreateMilestonePaymentSessionRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            CancellationToken cancellationToken)
    {
        await paymentSessionValidator.ValidateAndThrowBusinessExceptionAsync(
            request,
            cancellationToken);
        var payment = await paymentEscrowService
            .FundWithConfirmationTokenAsync(
                milestoneId,
                request.ConfirmationTokenReference,
                idempotencyKey,
                cancellationToken);
        return payment.Payment is null
            ? Accepted(ApiResponse<FundingOperationDto>.Ok(payment))
            : Ok(ApiResponse<FundingOperationDto>.Ok(payment));
    }

    [HttpGet("contracts/{contractId:guid}/payments")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialQuery)]
    [Authorize(
        Roles =
            "Client,Lawyer,FinanceAdministrator,SuperAdministrator")]
    public async Task<ActionResult<ApiResponse<PaymentHistoryDto>>>
        GetContractPaymentsAsync(
            Guid contractId,
            CancellationToken cancellationToken)
    {
        var payments =
            await paymentQueryService.GetContractPaymentsAsync(
                contractId,
                cancellationToken);
        return Ok(ApiResponse<PaymentHistoryDto>.Ok(payments));
    }

    [HttpGet("milestones/{milestoneId:guid}/payment")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialQuery)]
    [Authorize(
        Roles =
            "Client,Lawyer,FinanceAdministrator,SuperAdministrator")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>>
        GetMilestonePaymentAsync(
            Guid milestoneId,
            CancellationToken cancellationToken)
    {
        var payment =
            await paymentQueryService.GetMilestonePaymentAsync(
                milestoneId,
                cancellationToken);
        return Ok(ApiResponse<PaymentDto>.Ok(payment));
    }

    [HttpPost("payments/{paymentTransactionId:guid}/retry")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialMutation)]
    [Authorize(
        Roles = "FinanceAdministrator,SuperAdministrator")]
    public async Task<ActionResult<ApiResponse<FundingOperationDto>>> RetryAsync(
        Guid paymentTransactionId,
        [FromBody] RetryPaymentRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        request = request with
        {
            IdempotencyKey = idempotencyKey ?? string.Empty
        };
        await retryValidator.ValidateAndThrowBusinessExceptionAsync(
            request,
            cancellationToken);
        var payment = await paymentEscrowService.RetryAsync(
            paymentTransactionId,
            request.PaymentMethodReference,
            request.IdempotencyKey,
            cancellationToken);
        return payment.Payment is null
            ? Accepted(ApiResponse<FundingOperationDto>.Ok(payment))
            : Ok(ApiResponse<FundingOperationDto>.Ok(payment));
    }

    [HttpPost("payments/{paymentTransactionId:guid}/retry-session")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialMutation)]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ApiResponse<FundingOperationDto>>>
        RetryPaymentSessionAsync(
            Guid paymentTransactionId,
            [FromBody] RetryPaymentSessionRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            CancellationToken cancellationToken)
    {
        request = request with { IdempotencyKey = idempotencyKey ?? string.Empty };
        await retrySessionValidator.ValidateAndThrowBusinessExceptionAsync(
            request,
            cancellationToken);
        var payment = await paymentEscrowService
            .RetryWithConfirmationTokenAsync(
                paymentTransactionId,
                request.ConfirmationTokenReference,
                request.IdempotencyKey,
                cancellationToken);
        return payment.Payment is null
            ? Accepted(ApiResponse<FundingOperationDto>.Ok(payment))
            : Ok(ApiResponse<FundingOperationDto>.Ok(payment));
    }

    [HttpPost("payments/webhook")]
    [SecurityRateLimit(RateLimitPolicyNames.PaymentWebhook)]
    [AllowAnonymous]
    [ProducesResponseType(
        typeof(ApiResponse<string>),
        StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<ApiResponse<PaymentActionResultDto>>>
        HandleWebhookAsync(
            [FromHeader(Name = "X-Payment-Event-Id")]
            string? eventId,
            [FromHeader(Name = "X-Payment-Timestamp")]
            string? timestamp,
            [FromHeader(Name = "X-Payment-Signature")]
            string? signature,
            CancellationToken cancellationToken)
    {
        var providerOptions = paymentProviderOptions.Value;
        EnsureTrustedWebhookSource(
            HttpContext.Connection.RemoteIpAddress,
            providerOptions.WebhookAllowedIpRanges);
        var rawBody = await ReadBoundedBodyAsync(
            Request,
            providerOptions.WebhookMaximumBodySizeBytes,
            cancellationToken);
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            throw new BusinessException(
                "محتوى إشعار مزود الدفع مطلوب.");
        }

        PaymentWebhookRequest request;
        try
        {
            request = JsonSerializer.Deserialize<PaymentWebhookRequest>(
                    rawBody,
                    SerializerOptions)
                ?? throw new BusinessException(
                    "محتوى إشعار مزود الدفع غير صالح.");
        }
        catch (JsonException exception)
        {
            throw new BusinessException(
                "تعذر قراءة إشعار مزود الدفع. تحقق من صحة تنسيق البيانات.",
                exception);
        }

        await webhookValidator.ValidateAndThrowBusinessExceptionAsync(
            request,
            cancellationToken);
        var result = await paymentWebhookService.HandleWebhookAsync(
            request,
            eventId,
            timestamp,
            signature,
            rawBody,
            cancellationToken);
        return Ok(ApiResponse<PaymentActionResultDto>.Ok(result));
    }

    private static async Task<string> ReadBoundedBodyAsync(
        HttpRequest request,
        int maximumBodySizeBytes,
        CancellationToken cancellationToken)
    {
        if (request.ContentLength > maximumBodySizeBytes)
        {
            throw WebhookPayloadTooLarge();
        }

        using var body = new MemoryStream();
        var buffer = new byte[Math.Min(maximumBodySizeBytes + 1, 16_384)];
        while (true)
        {
            var bytesRead = await request.Body.ReadAsync(
                buffer,
                cancellationToken);
            if (bytesRead == 0)
            {
                break;
            }

            if (body.Length + bytesRead > maximumBodySizeBytes)
            {
                throw WebhookPayloadTooLarge();
            }

            await body.WriteAsync(
                buffer.AsMemory(0, bytesRead),
                cancellationToken);
        }

        return Encoding.UTF8.GetString(
            body.GetBuffer(),
            0,
            checked((int)body.Length));
    }

    private static void EnsureTrustedWebhookSource(
        IPAddress? remoteIpAddress,
        IReadOnlyCollection<string>? allowedIpRanges)
    {
        if (allowedIpRanges is null || allowedIpRanges.Count == 0)
        {
            return;
        }

        if (remoteIpAddress?.IsIPv4MappedToIPv6 == true)
        {
            remoteIpAddress = remoteIpAddress.MapToIPv4();
        }

        if (remoteIpAddress is not null
            && allowedIpRanges.Any(range =>
                IPNetwork.Parse(range).Contains(remoteIpAddress)))
        {
            return;
        }

        throw new ForbiddenAccessException(
            "مصدر إشعار مزود الدفع غير موثوق.");
    }

    private static PayloadTooLargeException WebhookPayloadTooLarge()
        => new(
            "يتجاوز حجم إشعار مزود الدفع الحد الأقصى المسموح به.");
}


