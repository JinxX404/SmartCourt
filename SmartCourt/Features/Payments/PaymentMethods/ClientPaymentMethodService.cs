using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using SmartCourt.Providers.Payments;

namespace SmartCourt.Features.Payments;

public sealed class ClientPaymentMethodService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IClientPaymentMethodProvider provider,
    IPaymentBrowserConfigurationProvider browserConfigurationProvider,
    IOptions<PaymentProviderOptions> paymentProviderOptions,
    TimeProvider timeProvider) : IClientPaymentMethodService
{
    public async Task<SetupPaymentMethodSessionDto> CreateSetupSessionAsync(
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        EnsureBrowserPaymentMethodsAvailable();
        var clientUserId = GetActorUserId();
        var key = RequireIdempotencyKey(idempotencyKey);
        var customer = await GetOrCreateCustomerAsync(
            clientUserId,
            cancellationToken);
        var result = await provider.CreateSetupIntentAsync(
            new ProviderSetupIntentRequest(
                clientUserId,
                customer.ProviderCustomerId,
                $"setup-{clientUserId:N}-{key}"),
            cancellationToken);
        if (result.IsLive != customer.IsLive
            || string.IsNullOrWhiteSpace(result.ClientSecret))
        {
            throw new BusinessException(
                "The payment provider returned an invalid saved-payment-method setup session.");
        }

        return new SetupPaymentMethodSessionDto(
            result.ProviderSetupIntentId,
            result.ClientSecret,
            result.Status);
    }

    public async Task<IReadOnlyList<SavedPaymentMethodDto>> ListAsync(
        CancellationToken cancellationToken)
    {
        EnsureBrowserPaymentMethodsAvailable();
        var clientUserId = GetActorUserId();
        var customer = await dbContext.ClientPaymentCustomers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.ClientUserId == clientUserId
                    && item.ProviderCode == ProviderCode,
                cancellationToken);
        if (customer is null)
        {
            return [];
        }

        var methods = await provider.ListPaymentMethodsAsync(
            customer.ProviderCustomerId,
            cancellationToken);
        return methods.Select(Map).ToList();
    }

    public Task SetDefaultAsync(
        string paymentMethodReference,
        CancellationToken cancellationToken)
        => MutateOwnedMethodAsync(
            paymentMethodReference,
            provider.SetDefaultPaymentMethodAsync,
            cancellationToken);

    public Task RemoveAsync(
        string paymentMethodReference,
        CancellationToken cancellationToken)
        => MutateOwnedMethodAsync(
            paymentMethodReference,
            provider.RemovePaymentMethodAsync,
            cancellationToken);

    private async Task MutateOwnedMethodAsync(
        string paymentMethodReference,
        Func<string, string, CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        EnsureBrowserPaymentMethodsAvailable();
        if (string.IsNullOrWhiteSpace(paymentMethodReference)
            || paymentMethodReference.Length > 200)
        {
            throw new BusinessException("A valid payment method reference is required.");
        }

        var customer = await dbContext.ClientPaymentCustomers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.ClientUserId == GetActorUserId()
                    && item.ProviderCode == ProviderCode,
                cancellationToken)
            ?? throw new NotFoundException(
                "No saved payment profile exists for this client.");
        await operation(
            customer.ProviderCustomerId,
            paymentMethodReference.Trim(),
            cancellationToken);
    }

    private async Task<ClientPaymentCustomer> GetOrCreateCustomerAsync(
        Guid clientUserId,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.ClientPaymentCustomers
            .SingleOrDefaultAsync(
                item => item.ClientUserId == clientUserId
                    && item.ProviderCode == ProviderCode,
                cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var user = await dbContext.Users.AsNoTracking().SingleAsync(
            item => item.Id == clientUserId,
            cancellationToken);
        var providerCustomer = await provider.CreateCustomerAsync(
            new ProviderCustomerCreateRequest(
                clientUserId,
                user.Email ?? string.Empty,
                user.FullName,
                $"customer-{clientUserId:N}"),
            cancellationToken);
        var created = new ClientPaymentCustomer(
            Guid.NewGuid(),
            clientUserId,
            ProviderCode,
            providerCustomer.ProviderCustomerId,
            providerCustomer.IsLive,
            timeProvider.GetUtcNow().UtcDateTime);
        dbContext.ClientPaymentCustomers.Add(created);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return created;
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();
            return await dbContext.ClientPaymentCustomers.SingleAsync(
                item => item.ClientUserId == clientUserId
                    && item.ProviderCode == ProviderCode,
                cancellationToken);
        }
    }

    private Guid GetActorUserId()
    {
        if (!currentUserService.IsAuthenticated
            || !currentUserService.UserId.HasValue)
        {
            throw new AuthenticationException("Authentication is required.");
        }

        return currentUserService.UserId.Value;
    }

    private static string RequireIdempotencyKey(string? value)
    {
        var key = value?.Trim();
        if (string.IsNullOrWhiteSpace(key)
            || key.Length > IdempotencyHeader.MaximumLength)
        {
            throw new BusinessException("A valid Idempotency-Key header is required.");
        }

        return key;
    }

    private static SavedPaymentMethodDto Map(ProviderPaymentMethodResult item)
        => new(
            item.ProviderPaymentMethodId,
            item.Type,
            item.Brand,
            item.Last4,
            item.ExpiryMonth,
            item.ExpiryYear,
            item.HolderName,
            item.IsDefault);

    private void EnsureBrowserPaymentMethodsAvailable()
    {
        var browser = browserConfigurationProvider.BrowserConfiguration;
        if (!browser.IsTestEnvironment
            || !browser.SavedPaymentMethodsEnabled
            || !string.Equals(
                ProviderCode,
                browser.ProviderCode,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenAccessException(
                "Browser payment-method management is not available for the active test provider.");
        }
    }

    private string ProviderCode => paymentProviderOptions.Value.ProviderCode;
}
