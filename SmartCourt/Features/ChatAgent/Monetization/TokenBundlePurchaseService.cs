using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Configuration;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.ChatAgent.Entities;
using SmartCourt.Features.ChatAgent.Monetization.DTOs;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using SmartCourt.Providers.Payments;
using SmartCourt.Common.Domain;

namespace SmartCourt.Features.ChatAgent.Monetization;

public sealed class TokenBundlePurchaseService : ITokenBundlePurchaseService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentProvider _paymentProvider;
    private readonly IOptions<PaymentProviderOptions> _paymentOptions;
    private readonly IOptions<List<TokenBundleOptions>> _bundleOptions;
    private readonly TimeProvider _timeProvider;

    public TokenBundlePurchaseService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IPaymentProvider paymentProvider,
        IOptions<PaymentProviderOptions> paymentOptions,
        IOptions<List<TokenBundleOptions>> bundleOptions,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _paymentProvider = paymentProvider;
        _paymentOptions = paymentOptions;
        _bundleOptions = bundleOptions;
        _timeProvider = timeProvider;
    }

    public async Task<TokenBundlePurchaseResponse> PurchaseBundleAsync(
        string bundleId,
        string confirmationTokenReference,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var clientId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        
        var key = idempotencyKey?.Trim();
        if (string.IsNullOrWhiteSpace(key) || key.Length > 200)
            throw new BusinessException("A non-empty Idempotency-Key header of at most 200 characters is required.");

        var bundle = _bundleOptions.Value.FirstOrDefault(b => b.Id == bundleId) 
                     ?? throw new NotFoundException("الباقة المطلوبة غير موجودة.");

        var existing = await _dbContext.TokenBundlePaymentTransactions
            .SingleOrDefaultAsync(item => item.ProviderName == ProviderCode
                && item.IdempotencyKey == key, cancellationToken);
        
        if (existing is not null)
        {
            if (existing.BundleId != bundleId || existing.OperationType != PaymentOperationType.Deposit)
                throw new ConflictException("The idempotency key belongs to a different payment operation.");
            
            return new TokenBundlePurchaseResponse(
                existing.Id,
                bundle.Id,
                bundle.Name,
                bundle.CreditAmount,
                bundle.PriceEgp,
                string.Empty, // Client action may be lost on replay, frontend should check status
                null
            );
        }

        var now = _timeProvider.GetUtcNow();
        var transaction = new TokenBundlePaymentTransaction
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            BundleId = bundle.Id,
            TokenAmount = CreditConverter.ToTokens(bundle.CreditAmount),
            PriceEgp = bundle.PriceEgp,
            OperationType = PaymentOperationType.Deposit,
            Status = PaymentTransactionStatus.Processing,
            ProviderName = ProviderCode,
            IdempotencyKey = key,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.TokenBundlePaymentTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var customerReference = await _dbContext.ClientPaymentCustomers.AsNoTracking()
            .Where(item => item.ClientUserId == clientId && item.ProviderCode == ProviderCode)
            .Select(item => item.ProviderCustomerId)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;

        var request = new ProviderDepositRequest(
            bundle.PriceEgp, "EGP", transaction.Id, key, transaction.Id,
            PaymentMethodReference: _paymentOptions.Value.UseMockProvider
                ? confirmationTokenReference.Trim()
                : string.Empty,
            ConfirmationTokenReference: confirmationTokenReference.Trim(),
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

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (result.Outcome == ProviderOperationOutcome.Failed)
            throw new ConflictException(result.FailureReason ?? "The payment provider rejected the payment.");

        return new TokenBundlePurchaseResponse(
            transaction.Id,
            bundle.Id,
            bundle.Name,
            bundle.CreditAmount,
            bundle.PriceEgp,
            result.ClientAction?.ClientSecret ?? string.Empty,
            result.ClientAction?.RedirectUrl
        );
    }

    public async Task<TokenBundlePurchaseListDto> GetPurchasesAsync(
        Guid clientId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TokenBundlePaymentTransactions
            .AsNoTracking()
            .Where(x => x.ClientId == clientId);

        int totalCount = await query.CountAsync(cancellationToken);

        var purchases = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TokenBundlePurchaseDto(
                x.Id,
                x.BundleId,
                x.PriceEgp,
                CreditConverter.ToCredits(x.TokenAmount),
                x.Status.ToString(),
                x.FailureReason,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new TokenBundlePurchaseListDto(purchases, totalCount);
    }

    private string ProviderCode => _paymentOptions.Value.ProviderCode;
}
