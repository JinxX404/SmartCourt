using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Persistence;
using SmartCourt.Providers.Payments;

namespace SmartCourt.Features.Payments;

public sealed class PaymentWebhookService(
    ApplicationDbContext dbContext,
    IPaymentEscrowService paymentEscrowService,
    IOptions<PaymentProviderOptions> options,
    ILogger<PaymentWebhookService> logger,
    TimeProvider timeProvider) : IPaymentWebhookService
{
    private readonly PaymentProviderOptions _options = options.Value;

    public async Task<PaymentActionResultDto> HandleWebhookAsync(
        PaymentWebhookRequest request,
        string? eventIdHeader,
        string? timestampHeader,
        string? signatureHeader,
        string rawBody,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidateWebhookAuthentication(
                request,
                eventIdHeader,
                timestampHeader,
                signatureHeader,
                rawBody);
        }
        catch (BusinessException)
        {
            logger.LogWarning(
                "Rejected payment webhook {EventId}: authentication failed.",
                request.EventId);
            throw;
        }

        var existingEvent = await dbContext.PaymentWebhookEvents
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.EventId == request.EventId,
                cancellationToken);
        if (existingEvent is not null)
        {
            if (existingEvent.PaymentTransactionId
                != request.PaymentTransactionId)
            {
                throw new BusinessException(
                    "تم استخدام معرّف حدث الدفع مسبقًا لمعاملة مختلفة.");
            }

            return new PaymentActionResultDto(
                request.PaymentTransactionId,
                "Duplicate",
                UtcNow);
        }

        var paymentTransaction = await dbContext.PaymentTransactions
            .SingleOrDefaultAsync(
                item =>
                    item.Id == request.PaymentTransactionId,
                cancellationToken)
            ?? throw new BusinessException(
                "معاملة الدفع المرتبطة بإشعار المزود غير موجودة.");
        try
        {
            EnsureWebhookMatchesTransaction(
                paymentTransaction,
                request);
        }
        catch (BusinessException)
        {
            logger.LogWarning(
                "Rejected payment webhook {EventId}: payload mismatch for transaction {PaymentTransactionId}.",
                request.EventId,
                request.PaymentTransactionId);
            throw;
        }

        if (paymentTransaction.Status
            != PaymentTransactionStatus.Processing)
        {
            return await RecordTerminalWebhookAsync(
                paymentTransaction,
                request,
                cancellationToken);
        }

        if (request.Status == PaymentTransactionStatus.Processing)
        {
            throw new BusinessException(
                "إشعار مزود الدفع لا يحتوي على نتيجة نهائية للمعاملة.");
        }

        var milestone = await dbContext.Milestones
            .SingleOrDefaultAsync(
                item => item.Id == paymentTransaction.MilestoneId,
                cancellationToken)
            ?? throw new BusinessException(
                "المرحلة المرتبطة بمعاملة الدفع غير موجودة.");
        if (milestone.ContractId != paymentTransaction.ContractId
            || milestone.Status
                != MilestoneStatus.FundingProcessing)
        {
            logger.LogWarning(
                "Rejected payment webhook {EventId}: milestone state or ownership mismatch for transaction {PaymentTransactionId}.",
                request.EventId,
                request.PaymentTransactionId);
            throw new BusinessException(
                "حالة المرحلة أو ارتباطها بمعاملة الدفع لا يسمحان بإكمال الإشعار.");
        }

        var contract = await dbContext.Contracts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == paymentTransaction.ContractId,
                cancellationToken)
            ?? throw new BusinessException(
                "العقد المرتبط بمعاملة الدفع غير موجود.");
        var now = UtcNow;
        var correlationId = Guid.NewGuid();
        var reservationId =
            await paymentEscrowService.FindProcessingFundingReservationIdAsync(
                milestone.Id,
                cancellationToken);
        dbContext.PaymentWebhookEvents.Add(
            new PaymentWebhookEvent(
                Guid.NewGuid(),
                request.EventId,
                paymentTransaction.Id,
                now));

        if (request.Status == PaymentTransactionStatus.Completed)
        {
            var providerResult = new ProviderResult(
                paymentTransaction.Amount,
                paymentTransaction.Currency,
                milestone.Id,
                paymentTransaction.IdempotencyKey,
                correlationId,
                ProviderOperationOutcome.Succeeded,
                request.ProviderTransactionId,
                null);
            try
            {
                await paymentEscrowService.CompleteFundingAsync(
                    milestone,
                    contract.LawyerUserId,
                    paymentTransaction,
                    providerResult,
                    reservationId,
                    null,
                    correlationId,
                    cancellationToken);
            }
            catch (BusinessException)
            {
                if (await WebhookEventExistsAsync(
                        request.EventId,
                        cancellationToken))
                {
                    return new PaymentActionResultDto(
                        paymentTransaction.Id,
                        "Duplicate",
                        UtcNow);
                }

                throw;
            }

            return new PaymentActionResultDto(
                paymentTransaction.Id,
                PaymentTransactionStatus.Completed.ToString(),
                now);
        }

        return await paymentEscrowService.FinalizeFailedExternalResultAsync(
            milestone,
            paymentTransaction,
            request.ProviderTransactionId,
            reservationId,
            correlationId,
            cancellationToken);
    }

    private void ValidateWebhookAuthentication(
        PaymentWebhookRequest request,
        string? eventIdHeader,
        string? timestampHeader,
        string? signatureHeader,
        string rawBody)
    {
        if (!string.Equals(
                eventIdHeader,
                request.EventId,
                StringComparison.Ordinal))
        {
            throw new BusinessException(
                "معرّف حدث الدفع في الترويسة لا يطابق محتوى الإشعار.");
        }

        if (!long.TryParse(
                timestampHeader,
                System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture,
                out var timestamp))
        {
            throw new BusinessException(
                "توقيت إشعار مزود الدفع غير صالح.");
        }

        var now = timeProvider.GetUtcNow();
        var sentAt = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        if (Math.Abs((now - sentAt).TotalSeconds) > 300)
        {
            throw new BusinessException(
                "انتهت صلاحية إشعار مزود الدفع أو أن توقيته خارج النطاق المسموح.");
        }

        var secret = _options.WebhookSecret;
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new BusinessException(
                "سر التحقق من إشعارات مزود الدفع غير مهيأ.");
        }

        if (string.IsNullOrWhiteSpace(signatureHeader)
            || !signatureHeader.StartsWith(
                "v1=",
                StringComparison.Ordinal))
        {
            throw new BusinessException(
                "توقيع إشعار مزود الدفع مفقود أو غير صالح.");
        }

        byte[] suppliedSignature;
        try
        {
            suppliedSignature = Convert.FromBase64String(
                signatureHeader[3..]);
        }
        catch (FormatException exception)
        {
            throw new BusinessException(
                "توقيع إشعار مزود الدفع غير صالح.",
                exception);
        }

        var signedPayload = Encoding.UTF8.GetBytes(
            $"{timestampHeader}.{rawBody}");
        var expectedSignature = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            signedPayload);
        if (suppliedSignature.Length != expectedSignature.Length
            || !CryptographicOperations.FixedTimeEquals(
                suppliedSignature,
                expectedSignature))
        {
            logger.LogWarning(
                "Rejected payment webhook {EventId}: invalid signature.",
                request.EventId);
            throw new BusinessException(
                "تعذر التحقق من توقيع إشعار مزود الدفع.");
        }
    }

    private static void EnsureWebhookMatchesTransaction(
        PaymentTransaction paymentTransaction,
        PaymentWebhookRequest request)
    {
        if (paymentTransaction.OperationType
                != PaymentOperationType.Deposit
            || !paymentTransaction.MilestoneId.HasValue)
        {
            throw new BusinessException(
                "إشعار التمويل لا يرتبط بمحاولة إيداع صالحة.");
        }

        if (request.Amount != paymentTransaction.Amount
            || !string.Equals(
                request.Currency,
                paymentTransaction.Currency,
                StringComparison.Ordinal)
            || !string.Equals(
                request.Currency,
                "EGP",
                StringComparison.Ordinal))
        {
            throw new BusinessException(
                "قيمة أو عملة إشعار الدفع لا تطابق معاملة التمويل الأصلية.");
        }

        if (!Enum.IsDefined(request.Status))
        {
            throw new BusinessException(
                "حالة إشعار مزود الدفع غير صالحة.");
        }

        if (string.IsNullOrWhiteSpace(
                request.ProviderTransactionId)
            || request.ProviderTransactionId.Length > 200)
        {
            throw new BusinessException(
                "معرّف معاملة مزود الدفع في الإشعار غير صالح.");
        }

        if (paymentTransaction.ProviderTransactionId is not null
            && !string.Equals(
                paymentTransaction.ProviderTransactionId,
                request.ProviderTransactionId,
                StringComparison.Ordinal))
        {
            throw new BusinessException(
                "معرّف معاملة مزود الدفع لا يطابق المحاولة الأصلية.");
        }
    }

    private async Task<PaymentActionResultDto> RecordTerminalWebhookAsync(
        PaymentTransaction paymentTransaction,
        PaymentWebhookRequest request,
        CancellationToken cancellationToken)
    {
        var now = UtcNow;
        if (paymentTransaction.Status == request.Status
            && (string.IsNullOrWhiteSpace(request.ProviderTransactionId)
                || string.Equals(
                    paymentTransaction.ProviderTransactionId,
                    request.ProviderTransactionId,
                    StringComparison.Ordinal)))
        {
            if (!await WebhookEventExistsAsync(
                    request.EventId,
                    cancellationToken))
            {
                dbContext.PaymentWebhookEvents.Add(
                    new PaymentWebhookEvent(
                        Guid.NewGuid(),
                        request.EventId,
                        paymentTransaction.Id,
                        now));
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return new PaymentActionResultDto(
                paymentTransaction.Id,
                "Duplicate",
                now);
        }

        throw new BusinessException(
            "تم حسم معاملة الدفع مسبقًا بحالة لا تطابق الإشعار الوارد.");
    }

    private async Task<bool> WebhookEventExistsAsync(
        string eventId,
        CancellationToken cancellationToken)
    {
        return await dbContext.PaymentWebhookEvents
            .AsNoTracking()
            .AnyAsync(
                item => item.EventId == eventId,
                cancellationToken);
    }

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;
}
