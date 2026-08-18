using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Infrastructure.Providers.Payments;

namespace SmartCourt.Providers.Payments.Stripe;

public sealed class StripePaymentProvider :
    IPaymentProvider,
    IPaymentReconciliationProvider,
    ILawyerPayoutAccountProvider,
    IClientPaymentMethodProvider,
    IPaymentBrowserConfigurationProvider
{
    private const string ObjectPaymentIntent = "payment_intent";
    private const string ObjectTransfer = "transfer";
    private const string ObjectRefund = "refund";
    private const string ObjectPayout = "payout";
    private readonly global::Stripe.PaymentIntentService _paymentIntents;
    private readonly global::Stripe.ChargeService _charges;
    private readonly global::Stripe.TransferService _transfers;
    private readonly global::Stripe.RefundService _refunds;
    private readonly global::Stripe.PayoutService _payouts;
    private readonly global::Stripe.BalanceService _balances;
    private readonly global::Stripe.BalanceTransactionService _balanceTransactions;
    private readonly global::Stripe.AccountService _accounts;
    private readonly global::Stripe.AccountLinkService _accountLinks;
    private readonly global::Stripe.AccountLoginLinkService _loginLinks;
    private readonly global::Stripe.CustomerService _customers;
    private readonly global::Stripe.SetupIntentService _setupIntents;
    private readonly global::Stripe.PaymentMethodService _paymentMethods;
    private readonly global::Stripe.V2.Core.AccountService _accountsV2;
    private readonly global::Stripe.V2.Core.AccountLinkService _accountLinksV2;
    private readonly ILogger<StripePaymentProvider> _logger;
    private readonly StripeOptions _options;

    public StripePaymentProvider(
        global::Stripe.StripeClient stripeClient,
        IOptions<StripeOptions> options,
        ILogger<StripePaymentProvider> logger)
    {
        _paymentIntents = new global::Stripe.PaymentIntentService(stripeClient);
        _charges = new global::Stripe.ChargeService(stripeClient);
        _transfers = new global::Stripe.TransferService(stripeClient);
        _refunds = new global::Stripe.RefundService(stripeClient);
        _payouts = new global::Stripe.PayoutService(stripeClient);
        _balances = new global::Stripe.BalanceService(stripeClient);
        _balanceTransactions =
            new global::Stripe.BalanceTransactionService(stripeClient);
        _accounts = new global::Stripe.AccountService(stripeClient);
        _accountLinks = new global::Stripe.AccountLinkService(stripeClient);
        _loginLinks = new global::Stripe.AccountLoginLinkService(stripeClient);
        _customers = new global::Stripe.CustomerService(stripeClient);
        _setupIntents = new global::Stripe.SetupIntentService(stripeClient);
        _paymentMethods = new global::Stripe.PaymentMethodService(stripeClient);
        _accountsV2 = stripeClient.V2.Core.Accounts;
        _accountLinksV2 = stripeClient.V2.Core.AccountLinks;
        _logger = logger;
        _options = options.Value;
    }

    public ProviderPayoutAccountSettings Settings => new(
        StripeOptions.ProviderCode,
        _options.SandboxOnly,
        _options.DefaultConnectedAccountCountry,
        _options.ConnectReturnUrl,
        _options.ConnectRefreshUrl);

    public ProviderBrowserConfiguration BrowserConfiguration => new(
        StripeOptions.ProviderCode,
        _options.PublishableKey,
        "EGP",
        _options.SandboxOnly,
        _options.SandboxOnly
            && _options.SecretKey.StartsWith("sk_test_", StringComparison.Ordinal)
            && _options.PublishableKey.StartsWith("pk_test_", StringComparison.Ordinal),
        ConfirmationTokensEnabled: true,
        SavedPaymentMethodsEnabled: true);

    public Task<ProviderResult> DepositAsync(
        ProviderDepositRequest request,
        CancellationToken cancellationToken)
        => CreateDepositAsync(
            request.Amount,
            request.Currency,
            request.BusinessId,
            request.ProviderIdempotencyKey,
            request.CorrelationId,
            request.PaymentMethodReference,
            request.ConfirmationTokenReference,
            request.CustomerReference,
            cancellationToken);

    public Task<ProviderResult> RetryDepositAsync(
        ProviderDepositRetryRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PaymentMethodReference)
            && string.IsNullOrWhiteSpace(request.ConfirmationTokenReference))
        {
            return Task.FromResult(Failed(
                request,
                "A new tokenized payment method is required for a failed payment retry.",
                "requires_payment_method",
                ObjectPaymentIntent));
        }

        return CreateDepositAsync(
            request.Amount,
            request.Currency,
            request.BusinessId,
            request.ProviderIdempotencyKey,
            request.CorrelationId,
            request.PaymentMethodReference,
            request.ConfirmationTokenReference,
            request.CustomerReference,
            cancellationToken);
    }

    public async Task<ProviderResult> ReleaseAsync(
        ProviderReleaseRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SourcePaymentProviderTransactionId)
            || string.IsNullOrWhiteSpace(request.DestinationAccountId))
        {
            return Failed(
                request,
                "The source payment and verified lawyer payout account are required.",
                "invalid_release_reference",
                ObjectTransfer);
        }

        try
        {
            var paymentIntent = await _paymentIntents.GetAsync(
                request.SourcePaymentProviderTransactionId,
                new global::Stripe.PaymentIntentGetOptions
                {
                    Expand = ["latest_charge"]
                },
                cancellationToken: cancellationToken);
            var chargeId = string.IsNullOrWhiteSpace(
                    request.SourceChargeProviderTransactionId)
                ? paymentIntent.LatestChargeId
                : request.SourceChargeProviderTransactionId;
            if (string.IsNullOrWhiteSpace(chargeId))
            {
                return Unknown(
                    request,
                    paymentIntent.Status,
                    ObjectTransfer,
                    "Stripe has not supplied a source Charge for the deposit.");
            }

            var charge = await _charges.GetAsync(
                chargeId,
                new global::Stripe.ChargeGetOptions
                {
                    Expand = ["balance_transaction"]
                },
                cancellationToken: cancellationToken);
            var providerGross = charge.BalanceTransaction?.Amount
                ?? charge.Amount;
            var providerCurrency = charge.BalanceTransaction?.Currency
                ?? charge.Currency;
            var grossBusinessAmount = request.GrossBusinessAmount > 0m
                ? request.GrossBusinessAmount
                : request.Amount;
            var providerNet = AllocateProviderMinorAmount(
                providerGross,
                request.Amount,
                grossBusinessAmount);
            if (providerNet <= 0)
            {
                return Failed(
                    request,
                    "The calculated provider transfer amount is below one minor unit.",
                    "amount_too_small",
                    ObjectTransfer);
            }

            var transfer = await _transfers.CreateAsync(
                new global::Stripe.TransferCreateOptions
                {
                    Amount = providerNet,
                    Currency = providerCurrency,
                    Destination = request.DestinationAccountId,
                    SourceTransaction = charge.Id,
                    Description = "Mostashar lawyer milestone release",
                    Metadata = Metadata(
                        request.BusinessId,
                        request.CorrelationId,
                        request.ProviderIdempotencyKey,
                        "release")
                },
                Idempotent($"v2-{request.ProviderIdempotencyKey}"),
                cancellationToken);
            return Result(
                request,
                ProviderOperationOutcome.Succeeded,
                transfer.Id,
                null,
                "created",
                ObjectTransfer,
                new ProviderMoney(transfer.Amount, transfer.Currency),
                relatedProviderTransactionId: charge.Id);
        }
        catch (global::Stripe.StripeException exception)
        {
            return MapException(request, exception, ObjectTransfer);
        }
        catch (HttpRequestException exception)
        {
            return UnknownFromException(request, exception, ObjectTransfer);
        }
    }

    public async Task<ProviderResult> RefundAsync(
        ProviderRefundRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SourcePaymentProviderTransactionId))
        {
            return Failed(
                request,
                "The original Stripe PaymentIntent is required for a refund.",
                "invalid_refund_reference",
                ObjectRefund);
        }

        try
        {
            var refund = await _refunds.CreateAsync(
                new global::Stripe.RefundCreateOptions
                {
                    PaymentIntent = request.SourcePaymentProviderTransactionId,
                    Amount = ToMinorUnits(request.Amount, request.Currency),
                    Reason = "requested_by_customer",
                    Metadata = Metadata(
                        request.BusinessId,
                        request.CorrelationId,
                        request.ProviderIdempotencyKey,
                        "refund",
                        request.Reason)
                },
                Idempotent(request.ProviderIdempotencyKey),
                cancellationToken);
            return MapRefund(request, refund);
        }
        catch (global::Stripe.StripeException exception)
        {
            return MapException(request, exception, ObjectRefund);
        }
        catch (HttpRequestException exception)
        {
            return UnknownFromException(request, exception, ObjectRefund);
        }
    }

    public async Task<ProviderResult> WithdrawAsync(
        ProviderWithdrawalRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ConnectedAccountId)
            || request.PayoutMoney is null
            || request.PayoutMoney.AmountMinor <= 0)
        {
            return Failed(
                request,
                "يلزم وجود حساب سحب موثق ورصيد مخصص لدى مزود الدفع.",
                "invalid_payout_account",
                ObjectPayout);
        }

        try
        {
            var payout = await _payouts.CreateAsync(
                new global::Stripe.PayoutCreateOptions
                {
                    Amount = request.PayoutMoney.AmountMinor,
                    Currency = request.PayoutMoney.Currency,
                    Description = "Mostashar lawyer withdrawal",
                    Metadata = Metadata(
                        request.BusinessId,
                        request.CorrelationId,
                        request.ProviderIdempotencyKey,
                        "withdrawal")
                },
                new global::Stripe.RequestOptions
                {
                    IdempotencyKey = request.ProviderIdempotencyKey,
                    StripeAccount = request.ConnectedAccountId
                },
                cancellationToken);
            return MapPayout(request, payout);
        }
        catch (global::Stripe.StripeException exception)
        {
            return MapException(request, exception, ObjectPayout);
        }
        catch (HttpRequestException exception)
        {
            return UnknownFromException(request, exception, ObjectPayout);
        }
    }

    public async Task<ProviderResult?> GetDepositStatusAsync(
        ProviderDepositStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProviderTransactionId))
        {
            return null;
        }

        try
        {
            var paymentIntent = await _paymentIntents.GetAsync(
                request.ProviderTransactionId,
                cancellationToken: cancellationToken);
            return MapPaymentIntent(request, paymentIntent);
        }
        catch (global::Stripe.StripeException exception)
        {
            return MapException(request, exception, ObjectPaymentIntent);
        }
    }

    public async Task<ProviderResult?> GetReleaseStatusAsync(
        ProviderReleaseStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProviderTransactionId))
        {
            return null;
        }

        try
        {
            var transfer = await _transfers.GetAsync(
                request.ProviderTransactionId,
                cancellationToken: cancellationToken);
            var outcome = transfer.Reversed
                ? ProviderOperationOutcome.Failed
                : ProviderOperationOutcome.Succeeded;
            return Result(
                request,
                outcome,
                transfer.Id,
                transfer.Reversed ? "Stripe Transfer was fully reversed." : null,
                transfer.Reversed ? "reversed" : "created",
                ObjectTransfer,
                new ProviderMoney(transfer.Amount, transfer.Currency),
                relatedProviderTransactionId: transfer.SourceTransactionId);
        }
        catch (global::Stripe.StripeException exception)
        {
            return MapException(request, exception, ObjectTransfer);
        }
    }

    public async Task<ProviderResult?> GetRefundStatusAsync(
        ProviderRefundStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProviderTransactionId))
        {
            return null;
        }

        try
        {
            var refund = await _refunds.GetAsync(
                request.ProviderTransactionId,
                cancellationToken: cancellationToken);
            return MapRefund(request, refund);
        }
        catch (global::Stripe.StripeException exception)
        {
            return MapException(request, exception, ObjectRefund);
        }
    }

    public async Task<ProviderResult?> GetWithdrawalStatusAsync(
        ProviderWithdrawalStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProviderTransactionId)
            || string.IsNullOrWhiteSpace(request.ConnectedAccountId))
        {
            return null;
        }

        try
        {
            var payout = await _payouts.GetAsync(
                request.ProviderTransactionId,
                requestOptions: new global::Stripe.RequestOptions
                {
                    StripeAccount = request.ConnectedAccountId
                },
                cancellationToken: cancellationToken);
            return MapPayout(request, payout);
        }
        catch (global::Stripe.StripeException exception)
        {
            return MapException(request, exception, ObjectPayout);
        }
    }

    public async Task<ProviderPayoutAccountResult> CreateAccountAsync(
        ProviderPayoutAccountCreateRequest request,
        CancellationToken cancellationToken)
    {
        var account = await _accountsV2.CreateAsync(
            new global::Stripe.V2.Core.AccountCreateOptions
            {
                ContactEmail = request.Email,
                DisplayName = request.Email,
                Dashboard = "express",
                Identity = new global::Stripe.V2.Core.AccountCreateIdentityOptions
                {
                    Country = request.Country.ToLowerInvariant(),
                    EntityType = "individual"
                },
                Configuration = new global::Stripe.V2.Core.AccountCreateConfigurationOptions
                {
                    Recipient = new global::Stripe.V2.Core.AccountCreateConfigurationRecipientOptions
                    {
                        Capabilities = new global::Stripe.V2.Core.AccountCreateConfigurationRecipientCapabilitiesOptions
                        {
                            StripeBalance = new global::Stripe.V2.Core.AccountCreateConfigurationRecipientCapabilitiesStripeBalanceOptions
                            {
                                StripeTransfers = new global::Stripe.V2.Core.AccountCreateConfigurationRecipientCapabilitiesStripeBalanceStripeTransfersOptions
                                {
                                    Requested = true
                                }
                            }
                        }
                    }
                },
                Defaults = new global::Stripe.V2.Core.AccountCreateDefaultsOptions
                {
                    Currency = "usd",
                    Responsibilities = new global::Stripe.V2.Core.AccountCreateDefaultsResponsibilitiesOptions
                    {
                        FeesCollector = "application",
                        LossesCollector = "application"
                    }
                },
                Include = ["configuration.recipient", "identity", "requirements", "defaults"],
                Metadata = new Dictionary<string, string>
                {
                    ["smart_court_lawyer_user_id"] = request.LawyerUserId.ToString("N")
                }
            },
            Idempotent(request.ProviderIdempotencyKey),
            cancellationToken);
        await _accounts.UpdateAsync(
            account.Id,
            new global::Stripe.AccountUpdateOptions
            {
                Settings = new global::Stripe.AccountSettingsOptions
                {
                    Payouts = new global::Stripe.AccountSettingsPayoutsOptions
                    {
                        Schedule = new global::Stripe.AccountSettingsPayoutsScheduleOptions
                        {
                            Interval = "manual"
                        }
                    }
                }
            },
            Idempotent($"schedule-{request.ProviderIdempotencyKey}"),
            cancellationToken);
        return MapAccount(account);
    }

    public async Task<ProviderPayoutAccountResult> GetAccountAsync(
        string providerAccountId,
        CancellationToken cancellationToken)
    {
        var account = await _accountsV2.GetAsync(
            providerAccountId,
            new global::Stripe.V2.Core.AccountGetOptions
            {
                Include = ["configuration.recipient", "identity", "requirements", "defaults"]
            },
            cancellationToken: cancellationToken);
        return MapAccount(account);
    }

    public async Task<ProviderPayoutBalanceResult> GetBalanceAsync(
        string providerAccountId,
        string currency,
        long requiredAmountMinor,
        CancellationToken cancellationToken)
    {
        var normalizedCurrency = currency.Trim().ToLowerInvariant();
        var balance = await _balances.GetAsync(
            new global::Stripe.RequestOptions
            {
                StripeAccount = providerAccountId
            },
            cancellationToken);
        var available = balance.Available.SingleOrDefault(item =>
            string.Equals(
                item.Currency,
                normalizedCurrency,
                StringComparison.OrdinalIgnoreCase));
        var pending = balance.Pending.SingleOrDefault(item =>
            string.Equals(
                item.Currency,
                normalizedCurrency,
                StringComparison.OrdinalIgnoreCase));
        var availableAmount = available?.Amount ?? 0L;
        var pendingAmount = pending?.Amount ?? 0L;
        DateTimeOffset? expectedAvailableAt = null;
        if (pendingAmount > 0 && (requiredAmountMinor <= 0 || availableAmount < requiredAmountMinor))
        {
            var transactions = await _balanceTransactions.ListAsync(
                new global::Stripe.BalanceTransactionListOptions
                {
                    Currency = normalizedCurrency,
                    Limit = 100
                },
                new global::Stripe.RequestOptions
                {
                    StripeAccount = providerAccountId
                },
                cancellationToken);
            decimal accumulatedPending = 0m;
            var shortfall = requiredAmountMinor > 0 && availableAmount < requiredAmountMinor
                ? requiredAmountMinor - availableAmount
                : 1;
            foreach (var transaction in transactions.Data
                         .Where(item =>
                             string.Equals(
                                 item.Status,
                                 "pending",
                                 StringComparison.OrdinalIgnoreCase)
                             && item.Net > 0)
                         .OrderBy(item => item.AvailableOn))
            {
                accumulatedPending += transaction.Net;
                if (accumulatedPending < shortfall)
                {
                    continue;
                }

                expectedAvailableAt = new DateTimeOffset(
                    DateTime.SpecifyKind(
                        transaction.AvailableOn,
                        DateTimeKind.Utc));
                break;
            }
        }

        return new ProviderPayoutBalanceResult(
            normalizedCurrency,
            availableAmount,
            pendingAmount,
            expectedAvailableAt);
    }

    public async Task<ProviderOnboardingLinkResult> CreateOnboardingLinkAsync(
        ProviderOnboardingLinkRequest request,
        CancellationToken cancellationToken)
    {
        var link = await _accountLinksV2.CreateAsync(
            new global::Stripe.V2.Core.AccountLinkCreateOptions
            {
                Account = request.ProviderAccountId,
                UseCase = new global::Stripe.V2.Core.AccountLinkCreateUseCaseOptions
                {
                    Type = "account_onboarding",
                    AccountOnboarding = new global::Stripe.V2.Core.AccountLinkCreateUseCaseAccountOnboardingOptions
                    {
                        Configurations = ["recipient"],
                        ReturnUrl = request.ReturnUrl,
                        RefreshUrl = request.RefreshUrl,
                        CollectionOptions = new global::Stripe.V2.Core.AccountLinkCreateUseCaseAccountOnboardingCollectionOptionsOptions
                        {
                            Fields = "eventually_due",
                            FutureRequirements = "include"
                        }
                    }
                }
            },
            Idempotent(request.ProviderIdempotencyKey),
            cancellationToken);
        return new ProviderOnboardingLinkResult(link.Url, link.ExpiresAt);
    }

    public async Task<string> CreateDashboardLinkAsync(
        string providerAccountId,
        CancellationToken cancellationToken)
    {
        var link = await _loginLinks.CreateAsync(
            providerAccountId,
            cancellationToken: cancellationToken);
        return link.Url;
    }

    private async Task<ProviderResult> CreateDepositAsync(
        decimal amount,
        string currency,
        Guid businessId,
        string idempotencyKey,
        Guid correlationId,
        string paymentMethodReference,
        string confirmationTokenReference,
        string customerReference,
        CancellationToken cancellationToken)
    {
        var request = new ProviderDepositRequest(
            amount,
            currency,
            businessId,
            idempotencyKey,
            correlationId,
            paymentMethodReference,
            confirmationTokenReference,
            customerReference);
        if (string.IsNullOrWhiteSpace(paymentMethodReference)
            && string.IsNullOrWhiteSpace(confirmationTokenReference))
        {
            return Failed(
                request,
                "A tokenized payment method is required.",
                "requires_payment_method",
                ObjectPaymentIntent);
        }

        try
        {
            var paymentIntent = await _paymentIntents.CreateAsync(
                new global::Stripe.PaymentIntentCreateOptions
                {
                    Amount = ToMinorUnits(amount, currency),
                    Currency = currency.ToLowerInvariant(),
                    PaymentMethod = string.IsNullOrWhiteSpace(paymentMethodReference)
                        ? null
                        : paymentMethodReference.Trim(),
                    ConfirmationToken = string.IsNullOrWhiteSpace(confirmationTokenReference)
                        ? null
                        : confirmationTokenReference.Trim(),
                    Customer = string.IsNullOrWhiteSpace(customerReference)
                        ? null
                        : customerReference.Trim(),
                    AutomaticPaymentMethods =
                        new global::Stripe.PaymentIntentAutomaticPaymentMethodsOptions
                        {
                            Enabled = true,
                            AllowRedirects = "never"
                        },
                    Confirm = true,
                    CaptureMethod = "automatic",
                    UseStripeSdk = true,
                    Description = "Mostashar milestone funding",
                    TransferGroup = $"milestone_{businessId:N}",
                    Metadata = Metadata(
                        businessId,
                        correlationId,
                        idempotencyKey,
                        "deposit")
                },
                Idempotent(idempotencyKey),
                cancellationToken);
            return MapPaymentIntent(request, paymentIntent);
        }
        catch (global::Stripe.StripeException exception)
        {
            if (exception.StripeError?.PaymentIntent is { } paymentIntent)
            {
                return MapPaymentIntent(request, paymentIntent);
            }

            return MapException(request, exception, ObjectPaymentIntent);
        }
        catch (HttpRequestException exception)
        {
            return UnknownFromException(request, exception, ObjectPaymentIntent);
        }
    }

    private ProviderResult MapPaymentIntent(
        IProviderOperationRequest request,
        global::Stripe.PaymentIntent paymentIntent)
    {
        var outcome = paymentIntent.Status switch
        {
            "succeeded" => ProviderOperationOutcome.Succeeded,
            "requires_action" => ProviderOperationOutcome.RequiresCustomerAction,
            "processing" => ProviderOperationOutcome.Processing,
            "requires_payment_method" or "canceled" =>
                ProviderOperationOutcome.Failed,
            _ => ProviderOperationOutcome.Unknown
        };
        var action = outcome == ProviderOperationOutcome.RequiresCustomerAction
            ? new ProviderClientAction(
                ProviderClientActionType.ConfirmPayment,
                paymentIntent.ClientSecret)
            : null;
        var failure = outcome == ProviderOperationOutcome.Failed
            ? paymentIntent.LastPaymentError?.Message
                ?? "Stripe confirmed that the payment did not succeed."
            : null;
        return Result(
            request,
            outcome,
            paymentIntent.Id,
            failure,
            paymentIntent.Status,
            ObjectPaymentIntent,
            new ProviderMoney(paymentIntent.Amount, paymentIntent.Currency),
            action,
            paymentIntent.LatestChargeId);
    }

    private ProviderResult MapRefund(
        IProviderOperationRequest request,
        global::Stripe.Refund refund)
    {
        var outcome = refund.Status switch
        {
            "succeeded" => ProviderOperationOutcome.Succeeded,
            "pending" or "requires_action" => ProviderOperationOutcome.Processing,
            "failed" or "canceled" => ProviderOperationOutcome.Failed,
            _ => ProviderOperationOutcome.Unknown
        };
        return Result(
            request,
            outcome,
            refund.Id,
            outcome == ProviderOperationOutcome.Failed
                ? refund.FailureReason ?? "Stripe refund failed."
                : null,
            refund.Status,
            ObjectRefund,
            new ProviderMoney(refund.Amount, refund.Currency),
            relatedProviderTransactionId: refund.PaymentIntentId);
    }

    private ProviderResult MapPayout(
        IProviderOperationRequest request,
        global::Stripe.Payout payout)
    {
        var outcome = payout.Status switch
        {
            "paid" => ProviderOperationOutcome.Succeeded,
            "pending" or "in_transit" => ProviderOperationOutcome.Processing,
            "failed" or "canceled" => ProviderOperationOutcome.Failed,
            _ => ProviderOperationOutcome.Unknown
        };
        return Result(
            request,
            outcome,
            payout.Id,
            outcome == ProviderOperationOutcome.Failed
                ? PublicPayoutFailure(payout.FailureCode)
                : null,
            payout.Status,
            ObjectPayout,
            new ProviderMoney(payout.Amount, payout.Currency));
    }

    private ProviderResult MapException(
        IProviderOperationRequest request,
        global::Stripe.StripeException exception,
        string objectType)
    {
        var errorType = exception.StripeError?.Type;
        var deterministic = exception.HttpStatusCode is
                System.Net.HttpStatusCode.BadRequest
                or System.Net.HttpStatusCode.Unauthorized
                or System.Net.HttpStatusCode.PaymentRequired
                or System.Net.HttpStatusCode.Forbidden
                or System.Net.HttpStatusCode.NotFound
            && !string.Equals(errorType, "idempotency_error", StringComparison.Ordinal);
        var outcome = deterministic
            ? ProviderOperationOutcome.Failed
            : ProviderOperationOutcome.Unknown;
        _logger.LogWarning(
            exception,
            "Stripe {ObjectType} request failed. BusinessId: {BusinessId}; CorrelationId: {CorrelationId}; RequestId: {RequestId}; ErrorType: {ErrorType}; ErrorCode: {ErrorCode}.",
            objectType,
            request.BusinessId,
            request.CorrelationId,
            exception.StripeResponse?.RequestId,
            errorType,
            exception.StripeError?.Code);
        return Result(
            request,
            outcome,
            exception.StripeError?.PaymentIntent?.Id,
            SafeFailure(exception),
            exception.StripeError?.Code ?? errorType,
            objectType);
    }

    private ProviderResult UnknownFromException(
        IProviderOperationRequest request,
        Exception exception,
        string objectType)
    {
        _logger.LogWarning(
            exception,
            "Stripe {ObjectType} network result is unknown. BusinessId: {BusinessId}; CorrelationId: {CorrelationId}.",
            objectType,
            request.BusinessId,
            request.CorrelationId);
        return Unknown(
            request,
            "network_error",
            objectType,
            "تعذر التأكد من نتيجة العملية لدى مزود الدفع. يرجى المحاولة لاحقًا.");
    }

    public async Task<ProviderCustomerResult> CreateCustomerAsync(
        ProviderCustomerCreateRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await _customers.CreateAsync(
            new global::Stripe.CustomerCreateOptions
            {
                Email = request.Email,
                Name = request.Name,
                Description = "Mostashar client payment profile",
                Metadata = new Dictionary<string, string>
                {
                    ["smart_court_client_user_id"] = request.ClientUserId.ToString("N")
                }
            },
            Idempotent(request.ProviderIdempotencyKey),
            cancellationToken);
        return new ProviderCustomerResult(customer.Id, customer.Livemode);
    }

    public async Task<ProviderSetupIntentResult> CreateSetupIntentAsync(
        ProviderSetupIntentRequest request,
        CancellationToken cancellationToken)
    {
        var setupIntent = await _setupIntents.CreateAsync(
            new global::Stripe.SetupIntentCreateOptions
            {
                Customer = request.ProviderCustomerId,
                Usage = "on_session",
                AutomaticPaymentMethods = new global::Stripe.SetupIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                    AllowRedirects = "never"
                },
                Metadata = new Dictionary<string, string>
                {
                    ["smart_court_client_user_id"] = request.ClientUserId.ToString("N")
                }
            },
            Idempotent(request.ProviderIdempotencyKey),
            cancellationToken);
        return new ProviderSetupIntentResult(
            setupIntent.Id,
            setupIntent.ClientSecret,
            setupIntent.Status,
            setupIntent.Livemode);
    }

    public async Task<IReadOnlyList<ProviderPaymentMethodResult>> ListPaymentMethodsAsync(
        string providerCustomerId,
        CancellationToken cancellationToken)
    {
        var customer = await _customers.GetAsync(
            providerCustomerId,
            new global::Stripe.CustomerGetOptions
            {
                Expand = ["invoice_settings.default_payment_method"]
            },
            cancellationToken: cancellationToken);
        var methods = await _paymentMethods.ListAsync(
            new global::Stripe.PaymentMethodListOptions
            {
                Customer = providerCustomerId
            },
            cancellationToken: cancellationToken);
        return methods.Data.Select(method => new ProviderPaymentMethodResult(
            method.Id,
            method.Type,
            method.Card?.Brand,
            method.Card?.Last4,
            method.Card?.ExpMonth,
            method.Card?.ExpYear,
            method.BillingDetails?.Name,
            string.Equals(
                customer.InvoiceSettings?.DefaultPaymentMethodId,
                method.Id,
                StringComparison.Ordinal))).ToList();
    }

    public async Task SetDefaultPaymentMethodAsync(
        string providerCustomerId,
        string paymentMethodId,
        CancellationToken cancellationToken)
    {
        await EnsurePaymentMethodOwnedAsync(
            providerCustomerId,
            paymentMethodId,
            cancellationToken);
        await _customers.UpdateAsync(
            providerCustomerId,
            new global::Stripe.CustomerUpdateOptions
            {
                InvoiceSettings = new global::Stripe.CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = paymentMethodId
                }
            },
            cancellationToken: cancellationToken);
    }

    public async Task RemovePaymentMethodAsync(
        string providerCustomerId,
        string paymentMethodId,
        CancellationToken cancellationToken)
    {
        await EnsurePaymentMethodOwnedAsync(
            providerCustomerId,
            paymentMethodId,
            cancellationToken);
        await _paymentMethods.DetachAsync(
            paymentMethodId,
            cancellationToken: cancellationToken);
    }

    private async Task EnsurePaymentMethodOwnedAsync(
        string providerCustomerId,
        string paymentMethodId,
        CancellationToken cancellationToken)
    {
        var method = await _paymentMethods.GetAsync(
            paymentMethodId,
            cancellationToken: cancellationToken);
        if (!string.Equals(
                method.CustomerId,
                providerCustomerId,
                StringComparison.Ordinal))
        {
            throw new global::Stripe.StripeException(
                "The payment method does not belong to this Mostashar client.");
        }
    }

    private static ProviderPayoutAccountResult MapAccount(
        global::Stripe.V2.Core.Account account)
    {
        var transferStatus = account.Configuration?.Recipient?.Capabilities
            ?.StripeBalance?.StripeTransfers?.Status;
        var payoutStatus = account.Configuration?.Recipient?.Capabilities
            ?.StripeBalance?.Payouts?.Status;
        var transfersEnabled = string.Equals(
            transferStatus,
            "active",
            StringComparison.OrdinalIgnoreCase);
        var payoutsEnabled = string.Equals(
            payoutStatus,
            "active",
            StringComparison.OrdinalIgnoreCase);
        var detailsSubmitted = transfersEnabled && payoutsEnabled;
        var restricted = string.Equals(transferStatus, "restricted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(payoutStatus, "restricted", StringComparison.OrdinalIgnoreCase);
        var status = detailsSubmitted && transfersEnabled && payoutsEnabled
                ? "enabled"
                : restricted
                    ? "restricted"
                    : detailsSubmitted
                        ? "pending_capabilities"
                        : "onboarding";
        return new ProviderPayoutAccountResult(
            account.Id,
            status,
            detailsSubmitted,
            transfersEnabled,
            payoutsEnabled,
            account.Livemode,
            account.Identity?.Country ?? string.Empty,
            account.Defaults?.Currency ?? string.Empty,
            MaskedDestination: null);
    }

    private static long AllocateProviderMinorAmount(
        long providerGrossMinor,
        decimal netBusinessAmount,
        decimal grossBusinessAmount)
    {
        if (providerGrossMinor <= 0
            || netBusinessAmount <= 0m
            || grossBusinessAmount <= 0m
            || netBusinessAmount > grossBusinessAmount)
        {
            return 0;
        }

        return checked((long)decimal.Floor(
            providerGrossMinor * netBusinessAmount / grossBusinessAmount));
    }

    private static long ToMinorUnits(decimal amount, string currency)
    {
        if (!string.Equals(currency, "EGP", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Mostashar currently supports EGP business payments only.");
        }

        var minor = amount * 100m;
        if (minor != decimal.Truncate(minor))
        {
            throw new InvalidOperationException(
                "EGP amounts must not contain more than two decimal places.");
        }

        return checked((long)minor);
    }

    private static global::Stripe.RequestOptions Idempotent(string key)
        => new() { IdempotencyKey = key };

    private static Dictionary<string, string> Metadata(
        Guid businessId,
        Guid correlationId,
        string idempotencyKey,
        string operation,
        string? reason = null)
    {
        var metadata = new Dictionary<string, string>
        {
            ["smart_court_business_id"] = businessId.ToString("N"),
            ["smart_court_correlation_id"] = correlationId.ToString("N"),
            ["smart_court_idempotency_key"] = idempotencyKey,
            ["smart_court_operation"] = operation
        };
        if (!string.IsNullOrWhiteSpace(reason))
        {
            metadata["smart_court_reason"] = reason.Length <= 400
                ? reason
                : reason[..400];
        }

        return metadata;
    }

    private static string SafeFailure(global::Stripe.StripeException exception)
        => exception.StripeError?.Code switch
        {
            "balance_insufficient" or "insufficient_funds" =>
                "الرصيد المتاح لدى مزود الدفع لا يكفي لتنفيذ العملية حاليًا.",
            "card_declined" =>
                "تم رفض البطاقة من جهة الإصدار. يرجى استخدام بطاقة أخرى.",
            "currency_not_supported" =>
                "عملة العملية غير مدعومة لدى مزود الدفع.",
            _ => "تعذر على مزود الدفع إتمام العملية. يرجى المحاولة لاحقًا."
        };

    private static string PublicPayoutFailure(string? failureCode)
        => failureCode switch
        {
            "insufficient_funds" =>
                "الرصيد المتاح لدى مزود الدفع لا يكفي لتنفيذ السحب حاليًا.",
            "account_closed" =>
                "الحساب البنكي المرتبط مغلق. يرجى تحديث بيانات حساب السحب.",
            "no_account" or "invalid_account_number" =>
                "بيانات الحساب البنكي المرتبط غير صحيحة. يرجى تحديثها.",
            _ => "تعذر على مزود الدفع إتمام السحب. يرجى مراجعة حساب السحب والمحاولة لاحقًا."
        };

    private static ProviderResult Failed(
        IProviderOperationRequest request,
        string failure,
        string status,
        string objectType)
        => Result(
            request,
            ProviderOperationOutcome.Failed,
            null,
            failure,
            status,
            objectType);

    private static ProviderResult Unknown(
        IProviderOperationRequest request,
        string? status,
        string objectType,
        string failure)
        => Result(
            request,
            ProviderOperationOutcome.Unknown,
            null,
            failure,
            status,
            objectType);

    private static ProviderResult Result(
        IProviderOperationRequest request,
        ProviderOperationOutcome outcome,
        string? providerTransactionId,
        string? failureReason,
        string? providerStatus,
        string? providerObjectType,
        ProviderMoney? providerMoney = null,
        ProviderClientAction? clientAction = null,
        string? relatedProviderTransactionId = null)
        => new(
            request.Amount,
            request.Currency,
            request.BusinessId,
            request.ProviderIdempotencyKey,
            request.CorrelationId,
            outcome,
            providerTransactionId,
            failureReason,
            providerStatus,
            providerObjectType,
            providerMoney,
            clientAction,
            relatedProviderTransactionId);
}
