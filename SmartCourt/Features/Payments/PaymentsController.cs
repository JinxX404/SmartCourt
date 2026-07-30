using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments;

[ApiController]
[Route("api")]
[Authorize]
[Produces("application/json")]
public sealed class PaymentsController(
    IPaymentEscrowService paymentEscrowService,
    IValidator<RetryPaymentRequest> retryValidator,
    IValidator<PaymentWebhookRequest> webhookValidator) : ControllerBase
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    [HttpPost("milestones/{milestoneId:guid}/fund")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(
        typeof(ApiResponse<PaymentDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> FundAsync(
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
        return Ok(ApiResponse<PaymentDto>.Ok(payment));
    }

    [HttpGet("contracts/{contractId:guid}/payments")]
    [Authorize(
        Roles =
            "Client,Lawyer,FinanceAdministrator,SuperAdministrator")]
    [ProducesResponseType(
        typeof(ApiResponse<PaymentHistoryDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentHistoryDto>>>
        GetContractPaymentsAsync(
            Guid contractId,
            CancellationToken cancellationToken)
    {
        var payments =
            await paymentEscrowService.GetContractPaymentsAsync(
                contractId,
                cancellationToken);
        return Ok(ApiResponse<PaymentHistoryDto>.Ok(payments));
    }

    [HttpGet("milestones/{milestoneId:guid}/payment")]
    [Authorize(
        Roles =
            "Client,Lawyer,FinanceAdministrator,SuperAdministrator")]
    [ProducesResponseType(
        typeof(ApiResponse<PaymentDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>>
        GetMilestonePaymentAsync(
            Guid milestoneId,
            CancellationToken cancellationToken)
    {
        var payment =
            await paymentEscrowService.GetMilestonePaymentAsync(
                milestoneId,
                cancellationToken);
        return Ok(ApiResponse<PaymentDto>.Ok(payment));
    }

    [HttpPost("payments/{paymentTransactionId:guid}/retry")]
    [Authorize(
        Roles = "FinanceAdministrator,SuperAdministrator")]
    [ProducesResponseType(
        typeof(ApiResponse<PaymentDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> RetryAsync(
        Guid paymentTransactionId,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var request = new RetryPaymentRequest(
            idempotencyKey ?? string.Empty);
        await ValidateAsync(
            retryValidator,
            request,
            cancellationToken);
        var payment = await paymentEscrowService.RetryAsync(
            paymentTransactionId,
            request.IdempotencyKey,
            cancellationToken);
        return Ok(ApiResponse<PaymentDto>.Ok(payment));
    }

    [HttpPost("payments/webhook")]
    [AllowAnonymous]
    [ProducesResponseType(
        typeof(ApiResponse<PaymentActionResultDto>),
        StatusCodes.Status200OK)]
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
        using var reader = new StreamReader(
            Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
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

        await ValidateAsync(
            webhookValidator,
            request,
            cancellationToken);
        var result = await paymentEscrowService.HandleWebhookAsync(
            request,
            eventId,
            timestamp,
            signature,
            rawBody,
            cancellationToken);
        return Ok(ApiResponse<PaymentActionResultDto>.Ok(result));
    }

    private static async Task ValidateAsync<T>(
        IValidator<T> validator,
        T request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(
            request,
            cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new BusinessException(
                string.Join(
                    " ",
                    validationResult.Errors
                        .Select(error => error.ErrorMessage)
                        .Distinct(StringComparer.Ordinal)));
        }
    }
}
