using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Consultations.Payments;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Payments;

public sealed class PaymentProviderWebhookService(
    ApplicationDbContext dbContext,
    IPaymentReconciliationService paymentReconciliationService,
    IWalletService walletService,
    ILawyerPayoutAccountService payoutAccountService,
    IConsultationPaymentService consultationPaymentService,
    Microsoft.Extensions.Options.IOptions<SmartCourt.Providers.Payments.PaymentProviderOptions> options,
    TimeProvider timeProvider,
    ILogger<PaymentProviderWebhookService> logger)
{
    private readonly SmartCourt.Providers.Payments.PaymentProviderOptions
        _options = options.Value;

    public async Task<PaymentActionResultDto> HandleAsync(
        ProviderWebhookEvent providerEvent,
        CancellationToken cancellationToken)
    {
        if (providerEvent.IsLive)
        {
            throw new ForbiddenAccessException(
                "Live payment-provider events are disabled for this sandbox MVP.");
        }

        var storedEvent = await dbContext.PaymentWebhookEvents
            .SingleOrDefaultAsync(
                item => item.EventId == providerEvent.EventId,
                cancellationToken);
        if (storedEvent?.ProcessedAt is not null)
        {
            return new PaymentActionResultDto(
                storedEvent.Id,
                "Duplicate",
                storedEvent.ProcessedAt.Value);
        }

        var paymentTransactionId = string.IsNullOrWhiteSpace(
                providerEvent.ProviderObjectId)
            ? null
            : await dbContext.PaymentTransactions
                .Where(item => item.ProviderTransactionId
                    == providerEvent.ProviderObjectId)
                .Select(item => (Guid?)item.Id)
                .SingleOrDefaultAsync(cancellationToken);
        if (storedEvent is null)
        {
            storedEvent = new PaymentWebhookEvent(
                Guid.NewGuid(),
                _options.ProviderCode,
                providerEvent.EventId,
                providerEvent.EventType,
                providerEvent.ProviderObjectId,
                providerEvent.ConnectedAccountId,
                paymentTransactionId,
                UtcNow);
            dbContext.PaymentWebhookEvents.Add(storedEvent);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                dbContext.ChangeTracker.Clear();
                storedEvent = await dbContext.PaymentWebhookEvents
                    .SingleAsync(
                        item => item.EventId == providerEvent.EventId,
                        cancellationToken);
                if (storedEvent.ProcessedAt.HasValue)
                {
                    return new PaymentActionResultDto(
                        storedEvent.Id,
                        "Duplicate",
                        storedEvent.ProcessedAt.Value);
                }
            }
        }

        try
        {
            if ((string.Equals(
                     providerEvent.EventType,
                     "account.updated",
                     StringComparison.Ordinal)
                 || providerEvent.EventType.StartsWith(
                     "v2.core.account",
                     StringComparison.Ordinal))
                && !string.IsNullOrWhiteSpace(providerEvent.ProviderObjectId))
            {
                await payoutAccountService.SynchronizeProviderAccountAsync(
                    providerEvent.ProviderObjectId,
                    cancellationToken);
            }
            else if (providerEvent.EventType.StartsWith(
                         "payout.",
                         StringComparison.Ordinal))
            {
                await walletService.ReconcilePendingWithdrawalsAsync(
                    cancellationToken);
            }
            else if (paymentTransactionId.HasValue)
            {
                await paymentReconciliationService
                    .ReconcileProviderTransactionAsync(
                        paymentTransactionId.Value,
                    cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(providerEvent.ProviderObjectId))
            {
                await consultationPaymentService.ReconcileProviderObjectAsync(
                    providerEvent.ProviderObjectId,
                    cancellationToken);
            }

            storedEvent = await dbContext.PaymentWebhookEvents.SingleAsync(
                item => item.Id == storedEvent.Id,
                cancellationToken);
            storedEvent.ProcessedAt = UtcNow;
            storedEvent.ProcessingError = null;
            await dbContext.SaveChangesAsync(cancellationToken);
            return new PaymentActionResultDto(
                storedEvent.Id,
                "Processed",
                storedEvent.ProcessedAt.Value);
        }
        catch (Exception exception)
        {
            dbContext.ChangeTracker.Clear();
            var failedEvent = await dbContext.PaymentWebhookEvents
                .SingleAsync(
                    item => item.Id == storedEvent.Id,
                    CancellationToken.None);
            failedEvent.ProcessingError = exception.Message.Length <= 1000
                ? exception.Message
                : exception.Message[..1000];
            await dbContext.SaveChangesAsync(CancellationToken.None);
            logger.LogWarning(
                exception,
                "Payment-provider webhook {EventId} could not be processed.",
                providerEvent.EventId);
            throw;
        }
    }

    private DateTimeOffset UtcNow => timeProvider.GetUtcNow();
}
