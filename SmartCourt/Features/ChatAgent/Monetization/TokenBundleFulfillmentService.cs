using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.ChatAgent.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;

namespace SmartCourt.Features.ChatAgent.Monetization;

public sealed class TokenBundleFulfillmentService : ITokenBundleFulfillmentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPaymentReconciliationProvider _reconciliationProvider;
    private readonly TimeProvider _timeProvider;

    public TokenBundleFulfillmentService(
        ApplicationDbContext dbContext,
        IPaymentReconciliationProvider reconciliationProvider,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _reconciliationProvider = reconciliationProvider;
        _timeProvider = timeProvider;
    }

    public async Task ReconcileProviderObjectAsync(
        string providerObjectId,
        CancellationToken cancellationToken)
    {
        var transaction = await _dbContext.TokenBundlePaymentTransactions.SingleOrDefaultAsync(
            item => item.ProviderTransactionId == providerObjectId
                && item.Status == PaymentTransactionStatus.Processing,
            cancellationToken);

        if (transaction is null)
            return;

        var correlation = Guid.NewGuid();
        ProviderResult? result = transaction.OperationType switch
        {
            PaymentOperationType.Deposit => await _reconciliationProvider.GetDepositStatusAsync(
                new(transaction.PriceEgp, "EGP", transaction.Id,
                    transaction.IdempotencyKey, correlation, transaction.ProviderTransactionId), cancellationToken),
            _ => null
        };

        if (result is null || result.Outcome is ProviderOperationOutcome.Unknown or ProviderOperationOutcome.Processing)
            return;

        transaction.ProviderStatus = result.ProviderStatus;
        transaction.FailureReason = result.FailureReason;
        transaction.UpdatedAt = _timeProvider.GetUtcNow();
        
        transaction.Status = result.Outcome switch
        {
            ProviderOperationOutcome.Succeeded => PaymentTransactionStatus.Completed,
            ProviderOperationOutcome.Failed => PaymentTransactionStatus.Failed,
            _ => PaymentTransactionStatus.Processing
        };

        if (transaction.Status != PaymentTransactionStatus.Processing)
            transaction.ProcessedAtUtc = _timeProvider.GetUtcNow();

        if (transaction.Status == PaymentTransactionStatus.Completed)
        {
            await CompleteFulfillmentAsync(transaction, cancellationToken);
        }
        else
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task CompleteFulfillmentAsync(
        TokenBundlePaymentTransaction transaction,
        CancellationToken cancellationToken)
    {
        // Add purchased tokens to the ledger
        var ledger = await _dbContext.QuotaLedgers
            .FirstOrDefaultAsync(x => x.ClientId == transaction.ClientId, cancellationToken);

        if (ledger == null)
        {
            ledger = QuotaLedger.Create(transaction.ClientId);
            ledger.AddBalance(transaction.TokenAmount);
            _dbContext.QuotaLedgers.Add(ledger);
        }
        else
        {
            ledger.AddBalance(transaction.TokenAmount);
        }

        // Record the transaction
        var quotaTransaction = QuotaTransaction.Create(
            Guid.NewGuid(),
            transaction.ClientId,
            transaction.TokenAmount,
            QuotaTransactionReason.BundlePurchase,
            transaction.Id.ToString(),
            _timeProvider.GetUtcNow()
        );
        _dbContext.QuotaTransactions.Add(quotaTransaction);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
