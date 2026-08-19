using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Configuration;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.LawyerSubscription.DTOs;
using SmartCourt.Features.LawyerSubscription.Entities;
using SmartCourt.Features.LawyerSubscription.Enums;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using SmartCourt.Providers.Payments;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Common.Domain;

namespace SmartCourt.Features.LawyerSubscription;

internal sealed class LawyerSubscriptionPaymentService : ILawyerSubscriptionPaymentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentProvider _paymentProvider;
    private readonly IPaymentReconciliationProvider _reconciliationProvider;
    private readonly IOptions<PaymentProviderOptions> _paymentOptions;
    private readonly IOptions<List<TokenBundleOptions>> _bundleOptions;
    private readonly IOptions<LawyerPlanOptions> _planOptions;
    private readonly ILawyerQuotaService _lawyerQuotaService;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<LawyerSubscriptionPaymentService> _logger;

    public LawyerSubscriptionPaymentService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IPaymentProvider paymentProvider,
        IPaymentReconciliationProvider reconciliationProvider,
        IOptions<PaymentProviderOptions> paymentOptions,
        IOptions<List<TokenBundleOptions>> bundleOptions,
        IOptions<LawyerPlanOptions> planOptions,
        ILawyerQuotaService lawyerQuotaService,
        TimeProvider timeProvider,
        ILogger<LawyerSubscriptionPaymentService> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _paymentProvider = paymentProvider;
        _reconciliationProvider = reconciliationProvider;
        _paymentOptions = paymentOptions;
        _bundleOptions = bundleOptions;
        _planOptions = planOptions;
        _lawyerQuotaService = lawyerQuotaService;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    private string ProviderCode => _paymentOptions.Value.ProviderCode;

    public async Task<LawyerPaymentCheckoutResponse> PurchaseBundleAsync(
        string bundleId, string confirmationTokenReference, string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        var lawyerId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        
        var key = idempotencyKey?.Trim();
        if (string.IsNullOrWhiteSpace(key) || key.Length > 200)
            throw new BusinessException("مطلوب توفير مفتاح منع تكرار الطلب صالح وألا يتجاوز 200 حرف.");

        var bundle = _bundleOptions.Value.FirstOrDefault(b => b.Id == bundleId) 
                     ?? throw new NotFoundException("الباقة المطلوبة غير موجودة.");

        var existing = await _dbContext.LawyerPaymentTransactions
            .SingleOrDefaultAsync(item => item.ProviderName == ProviderCode
                && item.IdempotencyKey == key, cancellationToken);
        
        if (existing is not null)
        {
            if (existing.TargetId != bundleId || existing.TargetType != "Bundle")
                throw new ConflictException("مفتاح منع تكرار الطلب ينتمي إلى عملية دفع مختلفة.");
            
            return new LawyerPaymentCheckoutResponse(
                existing.Id.ToString(), bundle.Id, "Bundle", bundle.PriceEgp, string.Empty, null);
        }

        var now = _timeProvider.GetUtcNow();
        var transaction = new LawyerPaymentTransaction
        {
            Id = Guid.NewGuid(),
            LawyerId = lawyerId,
            TargetId = bundle.Id,
            TargetType = "Bundle",
            PriceEgp = bundle.PriceEgp,
            OperationType = PaymentOperationType.Deposit,
            Status = PaymentTransactionStatus.Processing,
            ProviderName = ProviderCode,
            IdempotencyKey = key,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.LawyerPaymentTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // We use ClientPaymentCustomers table for Stripe Customers as it's generic enough despite the name
        // (It connects a UserId to a ProviderCustomerId).
        var customerReference = await _dbContext.ClientPaymentCustomers.AsNoTracking()
            .Where(item => item.ClientUserId == lawyerId && item.ProviderCode == ProviderCode)
            .Select(item => item.ProviderCustomerId)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;

        var isPm = confirmationTokenReference.Trim().StartsWith("pm_", StringComparison.OrdinalIgnoreCase);
        var request = new ProviderDepositRequest(
            bundle.PriceEgp, "EGP", transaction.Id, key, transaction.Id,
            PaymentMethodReference: _paymentOptions.Value.UseMockProvider || isPm ? confirmationTokenReference.Trim() : string.Empty,
            ConfirmationTokenReference: !isPm ? confirmationTokenReference.Trim() : string.Empty,
            CustomerReference: customerReference);

        ProviderResult result;
        try
        {
            result = await _paymentProvider.DepositAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            transaction.FailureReason = "The payment provider outcome is unknown and requires reconciliation.";
            transaction.UpdatedAt = _timeProvider.GetUtcNow();
            await _dbContext.SaveChangesAsync(CancellationToken.None);
            throw new BusinessException(transaction.FailureReason, exception);
        }

        transaction.ProviderTransactionId = result.ProviderTransactionId;
        transaction.RelatedProviderTransactionId = result.RelatedProviderTransactionId;
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
            await _lawyerQuotaService.RefundAsync(lawyerId, CreditConverter.ToTokens(bundle.CreditAmount), cancellationToken);
            _dbContext.LawyerQuotaTransactions.Add(new LawyerQuotaTransaction
            {
                Id = Guid.NewGuid(),
                LawyerId = lawyerId,
                Amount = CreditConverter.ToTokens(bundle.CreditAmount),
                Reason = $"Purchase of bundle {bundle.Name}",
                ReferenceId = transaction.Id.ToString(),
                CreatedAt = _timeProvider.GetUtcNow()
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (result.Outcome == ProviderOperationOutcome.Failed)
            throw new ConflictException(result.FailureReason ?? "The payment provider rejected the payment.");

        return new LawyerPaymentCheckoutResponse(
            transaction.Id.ToString(), bundle.Id, "Bundle", bundle.PriceEgp, result.ClientAction?.ClientSecret ?? string.Empty, result.ClientAction?.RedirectUrl);
    }

    public async Task<LawyerPaymentCheckoutResponse> PurchaseSubscriptionAsync(
        LawyerPlanType newPlan, string confirmationTokenReference, string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        var lawyerId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        
        var key = idempotencyKey?.Trim();
        if (string.IsNullOrWhiteSpace(key) || key.Length > 200)
            throw new BusinessException("مطلوب توفير مفتاح منع تكرار الطلب صالح وألا يتجاوز 200 حرف.");

        var planDef = _planOptions.Value.Plans.FirstOrDefault(p => p.PlanType.Equals(newPlan.ToString(), StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException("خطة الاشتراك المطلوبة غير موجودة.");

        if (planDef.MonthlyPriceEgp <= 0)
        {
            // Free plan upgrade immediately without Stripe checkout
            await _lawyerQuotaService.ChangeSubscriptionAsync(lawyerId, newPlan, cancellationToken);
            return new LawyerPaymentCheckoutResponse(Guid.NewGuid().ToString(), newPlan.ToString(), "Subscription", 0, string.Empty, null);
        }

        var existing = await _dbContext.LawyerPaymentTransactions
            .SingleOrDefaultAsync(item => item.ProviderName == ProviderCode
                && item.IdempotencyKey == key, cancellationToken);
        
        if (existing is not null)
        {
            if (existing.TargetId != newPlan.ToString() || existing.TargetType != "Subscription")
                throw new ConflictException("مفتاح منع تكرار الطلب ينتمي إلى عملية دفع مختلفة.");
            
            return new LawyerPaymentCheckoutResponse(
                existing.Id.ToString(), newPlan.ToString(), "Subscription", planDef.MonthlyPriceEgp, string.Empty, null);
        }

        var now = _timeProvider.GetUtcNow();
        var transaction = new LawyerPaymentTransaction
        {
            Id = Guid.NewGuid(),
            LawyerId = lawyerId,
            TargetId = newPlan.ToString(),
            TargetType = "Subscription",
            PriceEgp = planDef.MonthlyPriceEgp,
            OperationType = PaymentOperationType.Deposit,
            Status = PaymentTransactionStatus.Processing,
            ProviderName = ProviderCode,
            IdempotencyKey = key,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.LawyerPaymentTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var customerReference = await _dbContext.ClientPaymentCustomers.AsNoTracking()
            .Where(item => item.ClientUserId == lawyerId && item.ProviderCode == ProviderCode)
            .Select(item => item.ProviderCustomerId)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;

        var isPm = confirmationTokenReference.Trim().StartsWith("pm_", StringComparison.OrdinalIgnoreCase);
        var request = new ProviderDepositRequest(
            planDef.MonthlyPriceEgp, "EGP", transaction.Id, key, transaction.Id,
            PaymentMethodReference: _paymentOptions.Value.UseMockProvider || isPm ? confirmationTokenReference.Trim() : string.Empty,
            ConfirmationTokenReference: !isPm ? confirmationTokenReference.Trim() : string.Empty,
            CustomerReference: customerReference);

        ProviderResult result;
        try
        {
            result = await _paymentProvider.DepositAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            transaction.FailureReason = "The payment provider outcome is unknown and requires reconciliation.";
            transaction.UpdatedAt = _timeProvider.GetUtcNow();
            await _dbContext.SaveChangesAsync(CancellationToken.None);
            throw new BusinessException(transaction.FailureReason, exception);
        }

        transaction.ProviderTransactionId = result.ProviderTransactionId;
        transaction.RelatedProviderTransactionId = result.RelatedProviderTransactionId;
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
            await _lawyerQuotaService.ChangeSubscriptionAsync(lawyerId, newPlan, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (result.Outcome == ProviderOperationOutcome.Failed)
            throw new ConflictException(result.FailureReason ?? "The payment provider rejected the payment.");

        return new LawyerPaymentCheckoutResponse(
            transaction.Id.ToString(), newPlan.ToString(), "Subscription", planDef.MonthlyPriceEgp, result.ClientAction?.ClientSecret ?? string.Empty, result.ClientAction?.RedirectUrl);
    }

    public async Task ReconcileProviderObjectAsync(string providerObjectId, CancellationToken cancellationToken = default)
    {
        var transactions = await _dbContext.LawyerPaymentTransactions
            .Where(item => item.ProviderTransactionId == providerObjectId
                && item.Status == PaymentTransactionStatus.Processing)
            .ToListAsync(cancellationToken);

        if (transactions.Count == 0)
        {
            return;
        }

        var transaction = transactions.First();
        var correlation = Guid.NewGuid();

        ProviderResult? statusResponse = transaction.OperationType switch
        {
            PaymentOperationType.Deposit => await _reconciliationProvider.GetDepositStatusAsync(
                new(transaction.PriceEgp, "EGP", transaction.Id,
                    transaction.IdempotencyKey, correlation, transaction.ProviderTransactionId), cancellationToken),
            _ => null
        };

        if (statusResponse is null || statusResponse.Outcome == ProviderOperationOutcome.Unknown || statusResponse.Outcome == ProviderOperationOutcome.Processing)
        {
            return; // Not final
        }

        var newStatus = statusResponse.Outcome == ProviderOperationOutcome.Succeeded
            ? PaymentTransactionStatus.Completed
            : PaymentTransactionStatus.Failed;

        foreach (var txn in transactions)
        {
            txn.Status = newStatus;
            txn.ProviderStatus = statusResponse.ProviderStatus;
            txn.FailureReason = statusResponse.FailureReason;
            txn.UpdatedAt = _timeProvider.GetUtcNow();
            txn.ProcessedAtUtc = _timeProvider.GetUtcNow();
            
            if (newStatus == PaymentTransactionStatus.Completed)
            {
                if (txn.TargetType == "Bundle")
                {
                    var bundle = _bundleOptions.Value.FirstOrDefault(b => b.Id == txn.TargetId);
                    if (bundle != null)
                    {
                        await _lawyerQuotaService.RefundAsync(txn.LawyerId, CreditConverter.ToTokens(bundle.CreditAmount), cancellationToken);
                        
                        _dbContext.LawyerQuotaTransactions.Add(new LawyerQuotaTransaction
                        {
                            Id = Guid.NewGuid(),
                            LawyerId = txn.LawyerId,
                            Amount = CreditConverter.ToTokens(bundle.CreditAmount),
                            Reason = $"Purchase of bundle {bundle.Name}",
                            ReferenceId = txn.Id.ToString(),
                            CreatedAt = _timeProvider.GetUtcNow()
                        });
                    }
                }
                else if (txn.TargetType == "Subscription")
                {
                    if (Enum.TryParse<LawyerPlanType>(txn.TargetId, true, out var planType))
                    {
                        await _lawyerQuotaService.ChangeSubscriptionAsync(txn.LawyerId, planType, cancellationToken);
                    }
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
