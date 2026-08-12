using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Milestones.Domain;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.Settlement;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using SmartCourt.Providers.Payments;

namespace SmartCourt.Features.Payments;

public sealed class PaymentEscrowService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IContractService contractService,
    IContractUserEligibilityService userEligibilityService,
    IPaymentProvider paymentProvider,
    IIdempotencyService idempotencyService,
    IOutboxWriter outboxWriter,
    IOptions<PaymentProviderOptions> paymentProviderOptions,
    ILogger<PaymentEscrowService> logger,
    TimeProvider timeProvider) : IPaymentEscrowService
{
    private const string FundOperation = "FundMilestone";
    private const string RetryOperation = "RetryPayment";
    private const string MilestoneResource = "Milestone";
    private const string PaymentTransactionResource = "PaymentTransaction";
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<FundingOperationDto> FundWithConfirmationTokenAsync(
        Guid milestoneId,
        string confirmationTokenReference,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var customerReference = await GetClientCustomerReferenceAsync(
            actorUserId,
            cancellationToken);
        return await FundAsync(
            milestoneId,
            new FundMilestoneRequest(
                string.Empty,
                confirmationTokenReference,
                customerReference),
            idempotencyKey,
            cancellationToken);
    }

    public async Task<FundingOperationDto> FundAsync(
        Guid milestoneId,
        FundMilestoneRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        if (string.IsNullOrWhiteSpace(request.CustomerReference)
            && request.PaymentMethodReference.StartsWith(
                "pm_",
                StringComparison.Ordinal))
        {
            request = request with
            {
                CustomerReference = await ResolveCustomerForPaymentMethodAsync(
                    actorUserId,
                    request.PaymentMethodReference,
                    cancellationToken)
            };
        }
        var normalizedIdempotencyKey =
            RequireIdempotencyKey(idempotencyKey);
        var milestone = await dbContext.Milestones
            .SingleOrDefaultAsync(
                item => item.Id == milestoneId,
                cancellationToken)
            ?? throw new BusinessException(
                "المرحلة المطلوب تمويلها غير موجودة.");
        var contract = await contractService.GetAsync(
            milestone.ContractId,
            cancellationToken);

        if (contract.ClientUserId != actorUserId)
        {
            throw new ForbiddenAccessException(
                "عميل العقد فقط هو من يمكنه تمويل المرحلة.");
        }

        var scope = new IdempotencyScope(
            actorUserId,
            FundOperation,
            MilestoneResource,
            milestone.Id);
        var reservation = await idempotencyService.ReserveAsync(
            scope,
            normalizedIdempotencyKey,
            request,
            cancellationToken);
        if (reservation.IsReplay)
        {
            return await ReplayAsync(
                reservation,
                cancellationToken);
        }

        try
        {
            await EnsureFundingAllowedAsync(
                milestone,
                contract.Status,
                cancellationToken);
        }
        catch (BusinessException exception)
        {
            await FailReservationAsync(
                reservation.RecordId,
                null,
                exception.Message,
                cancellationToken);
            throw;
        }
        catch (ConflictException exception)
        {
            await FailReservationAsync(
                reservation.RecordId,
                null,
                exception.Message,
                cancellationToken);
            throw;
        }

        var now = UtcNow;
        var correlationId = Guid.NewGuid();
        var providerIdempotencyKey = CreateProviderIdempotencyKey(
            actorUserId,
            milestone.Id,
            normalizedIdempotencyKey);
        var paymentTransaction = new PaymentTransaction(
            Guid.NewGuid(),
            milestone.ContractId,
            milestone.Id,
            PaymentOperationType.Deposit,
            paymentProvider.GetType().Name,
            providerIdempotencyKey,
            milestone.Amount,
            now);

        MilestoneTransitionGuard.EnsureCanTransition(
            milestone.Status,
            MilestoneStatus.FundingProcessing);
        var previousStatus = milestone.Status;
        milestone.Status = MilestoneStatus.FundingProcessing;
        milestone.UpdatedAt = now;
        dbContext.PaymentTransactions.Add(paymentTransaction);
        AddHistory(
            milestone,
            previousStatus,
            MilestoneStatus.FundingProcessing,
            ContractPaymentEventTypes.MilestoneFundingStarted,
            actorUserId,
            "بدأت معالجة تمويل المرحلة.",
            correlationId,
            now);
        await EnqueueMilestoneEventAsync(
            ContractPaymentEventTypes.MilestoneFundingStarted,
            milestone.Id,
            correlationId,
            cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.ChangeTracker.Clear();
            await FailReservationAsync(
                reservation.RecordId,
                paymentTransaction.Id,
                "تم تعديل المرحلة بواسطة عملية أخرى قبل بدء التمويل.",
                cancellationToken);
            throw new ConflictException(
                "تم تعديل المرحلة بواسطة عملية أخرى. يرجى إعادة تحميلها والمحاولة مرة أخرى.");
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();
            await FailReservationAsync(
                reservation.RecordId,
                paymentTransaction.Id,
                "تعذر حجز عملية تمويل المرحلة.",
                cancellationToken);
            throw new ConflictException(
                "توجد عملية تمويل أخرى لهذه المرحلة أو لمفتاح الطلب نفسه.");
        }

        var providerRequest = new ProviderDepositRequest(
            milestone.Amount,
            contract.Currency,
            milestone.Id,
            providerIdempotencyKey,
            correlationId,
            request.PaymentMethodReference,
            request.ConfirmationTokenReference,
            request.CustomerReference);

        ProviderResult providerResult;
        try
        {
            providerResult = await paymentProvider.DepositAsync(
                providerRequest,
                cancellationToken);
        }
        catch (Exception exception)
        {
            await KeepProcessingForReconciliationAsync(
                paymentTransaction,
                "تعذر التأكد من نتيجة عملية الدفع لدى مزود الخدمة.",
                CancellationToken.None);
            throw new BusinessException(
                "تعذر التأكد من نتيجة تمويل المرحلة. لن تتم إعادة الخصم، وستتم مراجعة العملية تلقائيًا.",
                exception);
        }

        if (!ProviderResultMatches(
                providerResult,
                providerRequest))
        {
            await KeepProcessingForReconciliationAsync(
                paymentTransaction,
                "بيانات نتيجة مزود الدفع لا تطابق طلب التمويل.",
                cancellationToken);
            throw new BusinessException(
                "تعذر التحقق من نتيجة تمويل المرحلة. تم إيقاف أي محاولة جديدة لحين مراجعة العملية.");
        }

        return providerResult.Outcome switch
        {
            ProviderOperationOutcome.Succeeded =>
                CompletedFunding(await CompleteFundingAsync(
                    milestone,
                    contract.LawyerUserId,
                    paymentTransaction,
                    providerResult,
                    reservation.RecordId,
                    actorUserId,
                    correlationId,
                    cancellationToken), paymentTransaction),
            ProviderOperationOutcome.Failed =>
                await FailFundingAsync(
                    milestone,
                    paymentTransaction,
                    reservation.RecordId,
                    actorUserId,
                    correlationId,
                    cancellationToken),
            ProviderOperationOutcome.Unknown =>
                await KeepUnknownAndThrowAsync(
                    paymentTransaction,
                    cancellationToken),
            ProviderOperationOutcome.Processing
                or ProviderOperationOutcome.RequiresCustomerAction =>
                await KeepPendingFundingAsync(
                    milestone,
                    paymentTransaction,
                    providerResult,
                    reservation.RecordId,
                    cancellationToken),
            _ => throw new BusinessException(
                "أعاد مزود الدفع نتيجة غير صالحة لعملية تمويل المرحلة.")
        };
    }



    public async Task<FundingOperationDto> RetryAsync(
        Guid paymentTransactionId,
        string paymentMethodReference,
        string? idempotencyKey,
        CancellationToken cancellationToken)
        => await RetryCoreAsync(
            paymentTransactionId,
            paymentMethodReference,
            string.Empty,
            string.Empty,
            idempotencyKey,
            requireFinanceOperator: true,
            cancellationToken);

    public async Task<FundingOperationDto> RetryWithConfirmationTokenAsync(
        Guid paymentTransactionId,
        string confirmationTokenReference,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var customerReference = await GetClientCustomerReferenceAsync(
            actorUserId,
            cancellationToken);
        return await RetryCoreAsync(
            paymentTransactionId,
            string.Empty,
            confirmationTokenReference,
            customerReference,
            idempotencyKey,
            requireFinanceOperator: false,
            cancellationToken);
    }

    private async Task<FundingOperationDto> RetryCoreAsync(
        Guid paymentTransactionId,
        string paymentMethodReference,
        string confirmationTokenReference,
        string customerReference,
        string? idempotencyKey,
        bool requireFinanceOperator,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        if (requireFinanceOperator)
        {
            await EnsureFinanceOperatorAsync(
                actorUserId,
                cancellationToken);
        }
        if (paymentTransactionId == Guid.Empty)
        {
            throw new BusinessException(
                "معرّف معاملة الدفع مطلوب لإعادة المحاولة.");
        }

        var normalizedIdempotencyKey =
            RequireIdempotencyKey(idempotencyKey);
        var originalTransaction =
            await dbContext.PaymentTransactions
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.Id == paymentTransactionId,
                    cancellationToken)
            ?? throw new NotFoundException(
                "معاملة الدفع المطلوب إعادة محاولتها غير موجودة.");
        if (originalTransaction.Status
            != PaymentTransactionStatus.Failed)
        {
            throw new BusinessException(
                "يمكن إعادة محاولة معاملات الدفع التي أكد مزود الخدمة فشلها فقط.");
        }

        if (originalTransaction.OperationType
                != PaymentOperationType.Deposit
            || !originalTransaction.MilestoneId.HasValue)
        {
            throw new BusinessException(
                "إعادة المحاولة متاحة حاليًا لمعاملات تمويل المراحل فقط.");
        }

        var milestone = await dbContext.Milestones
            .SingleOrDefaultAsync(
                item =>
                    item.Id
                        == originalTransaction.MilestoneId.Value,
                cancellationToken)
            ?? throw new NotFoundException(
                "المرحلة المرتبطة بمعاملة الدفع غير موجودة.");
        var contract = await dbContext.Contracts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == originalTransaction.ContractId,
                cancellationToken)
            ?? throw new NotFoundException(
                "العقد المرتبط بمعاملة الدفع غير موجود.");
        if (!requireFinanceOperator && contract.ClientUserId != actorUserId)
        {
            throw new ForbiddenAccessException(
                "عميل العقد فقط هو من يمكنه إعادة محاولة دفع المرحلة.");
        }

        var scope = new IdempotencyScope(
            actorUserId,
            RetryOperation,
            PaymentTransactionResource,
            originalTransaction.Id);
        object retryRequest = string.IsNullOrWhiteSpace(
                confirmationTokenReference)
            ? new RetryPaymentRequest(
                paymentMethodReference,
                normalizedIdempotencyKey)
            : new RetryPaymentSessionRequest(
                confirmationTokenReference,
                normalizedIdempotencyKey);
        var reservation = await idempotencyService.ReserveAsync(
            scope,
            normalizedIdempotencyKey,
            retryRequest,
            cancellationToken);
        if (reservation.IsReplay)
        {
            return await ReplayAsync(
                reservation,
                cancellationToken);
        }

        try
        {
            await EnsureFundingAllowedAsync(
                milestone,
                contract.Status,
                cancellationToken);
        }
        catch (BusinessException exception)
        {
            await FailReservationAsync(
                reservation.RecordId,
                null,
                exception.Message,
                cancellationToken);
            throw;
        }
        catch (ConflictException exception)
        {
            await FailReservationAsync(
                reservation.RecordId,
                null,
                exception.Message,
                cancellationToken);
            throw;
        }

        var now = UtcNow;
        var correlationId = Guid.NewGuid();
        var providerIdempotencyKey =
            CreateRetryProviderIdempotencyKey(
                originalTransaction.IdempotencyKey,
                normalizedIdempotencyKey);
        var retryTransaction = new PaymentTransaction(
            Guid.NewGuid(),
            originalTransaction.ContractId,
            milestone.Id,
            PaymentOperationType.Deposit,
            paymentProvider.GetType().Name,
            providerIdempotencyKey,
            originalTransaction.Amount,
            now);

        var previousStatus = milestone.Status;
        MilestoneTransitionGuard.EnsureCanTransition(
            previousStatus,
            MilestoneStatus.FundingProcessing);
        milestone.Status = MilestoneStatus.FundingProcessing;
        milestone.UpdatedAt = now;
        dbContext.PaymentTransactions.Add(retryTransaction);
        AddHistory(
            milestone,
            previousStatus,
            MilestoneStatus.FundingProcessing,
            ContractPaymentEventTypes.MilestoneFundingStarted,
            actorUserId,
            "بدأ مسؤول العمليات المالية إعادة محاولة تمويل المرحلة.",
            correlationId,
            now);
        await EnqueueMilestoneEventAsync(
            ContractPaymentEventTypes.MilestoneFundingStarted,
            milestone.Id,
            correlationId,
            cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();
            await FailReservationAsync(
                reservation.RecordId,
                null,
                "تعذر حجز إعادة محاولة معاملة الدفع.",
                cancellationToken);
            throw new ConflictException(
                "بدأت عملية أخرى إعادة محاولة معاملة الدفع نفسها. يرجى إعادة تحميل البيانات.");
        }

        var providerRequest = new ProviderDepositRetryRequest(
            retryTransaction.Amount,
            retryTransaction.Currency,
            milestone.Id,
            providerIdempotencyKey,
            correlationId,
            originalTransaction.IdempotencyKey,
            originalTransaction.ProviderTransactionId,
            paymentMethodReference,
            confirmationTokenReference,
            customerReference);
        ProviderResult providerResult;
        try
        {
            providerResult = await paymentProvider.RetryDepositAsync(
                providerRequest,
                cancellationToken);
        }
        catch (Exception exception)
        {
            await KeepProcessingForReconciliationAsync(
                retryTransaction,
                "تعذر التأكد من نتيجة إعادة محاولة الدفع لدى مزود الخدمة.",
                CancellationToken.None);
            throw new BusinessException(
                "تعذر التأكد من نتيجة إعادة محاولة الدفع. لن تتكرر العملية تلقائيًا قبل المطابقة.",
                exception);
        }

        if (!ProviderResultMatches(
                providerResult,
                providerRequest))
        {
            await KeepProcessingForReconciliationAsync(
                retryTransaction,
                "بيانات نتيجة مزود الدفع لا تطابق إعادة المحاولة.",
                cancellationToken);
            throw new BusinessException(
                "تعذر التحقق من نتيجة إعادة محاولة الدفع. ستتم مراجعة العملية تلقائيًا.");
        }

        return providerResult.Outcome switch
        {
            ProviderOperationOutcome.Succeeded =>
                CompletedFunding(await CompleteFundingAsync(
                    milestone,
                    contract.LawyerUserId,
                    retryTransaction,
                    providerResult,
                    reservation.RecordId,
                    actorUserId,
                    correlationId,
                    cancellationToken), retryTransaction),
            ProviderOperationOutcome.Failed =>
                await FailFundingAsync(
                    milestone,
                    retryTransaction,
                    reservation.RecordId,
                    actorUserId,
                    correlationId,
                    cancellationToken),
            ProviderOperationOutcome.Unknown =>
                await KeepUnknownAndThrowAsync(
                    retryTransaction,
                    cancellationToken),
            ProviderOperationOutcome.Processing
                or ProviderOperationOutcome.RequiresCustomerAction =>
                await KeepPendingFundingAsync(
                    milestone,
                    retryTransaction,
                    providerResult,
                    reservation.RecordId,
                    cancellationToken),
            _ => throw new BusinessException(
                "أعاد مزود الدفع نتيجة غير صالحة لإعادة محاولة التمويل.")
        };
    }





    public async Task<PaymentDto> CompleteFundingAsync(
        Milestone milestone,
        Guid lawyerUserId,
        PaymentTransaction paymentTransaction,
        ProviderResult providerResult,
        Guid? reservationId,
        Guid? actorUserId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(
                providerResult.ProviderTransactionId)
            || providerResult.ProviderTransactionId.Length > 200)
        {
            await KeepProcessingForReconciliationAsync(
                paymentTransaction,
                "لم يرسل مزود الدفع معرّفًا صالحًا للمعاملة الناجحة.",
                cancellationToken);
            throw new BusinessException(
                "تعذر توثيق نتيجة تمويل المرحلة. تم إيقاف أي محاولة جديدة لحين مراجعة العملية.");
        }

        var now = UtcNow;
        var breakdown = SettlementCalculator.Calculate(
            milestone.Amount,
            0m);
        var account = await dbContext.EscrowAccounts
            .SingleOrDefaultAsync(
                item => item.ContractId == milestone.ContractId,
                cancellationToken);
        if (account is null)
        {
            account = new EscrowAccount(
                Guid.NewGuid(),
                milestone.ContractId,
                now);
            dbContext.EscrowAccounts.Add(account);
        }

        var wallet = await dbContext.LawyerWallets
            .SingleOrDefaultAsync(
                item => item.LawyerUserId == lawyerUserId,
                cancellationToken);
        if (wallet is null)
        {
            wallet = new LawyerWallet(
                Guid.NewGuid(),
                lawyerUserId,
                now);
            dbContext.LawyerWallets.Add(wallet);
        }

        var hold = new EscrowHold(
            Guid.NewGuid(),
            account.Id,
            milestone.ContractId,
            milestone.Id,
            breakdown.GrossAmount,
            breakdown.PlatformFeeAmount,
            breakdown.LawyerNetAmount,
            paymentTransaction.Id,
            now,
            now);
        dbContext.EscrowHolds.Add(hold);

        account.TotalDeposited += breakdown.GrossAmount;
        account.UpdatedAt = now;
        wallet.PendingBalance += breakdown.LawyerNetAmount;
        wallet.UpdatedAt = now;

        paymentTransaction.EscrowHoldId = hold.Id;
        paymentTransaction.ProviderTransactionId =
            providerResult.ProviderTransactionId;
        ApplyProviderResult(paymentTransaction, providerResult);
        paymentTransaction.Status =
            PaymentTransactionStatus.Completed;
        paymentTransaction.FailureReason = null;
        paymentTransaction.ProcessedAt = now;
        paymentTransaction.UpdatedAt = now;

        var runningBalance = account.TotalDeposited
            - account.TotalReleased
            - account.TotalRefunded
            - account.TotalFees;
        dbContext.EscrowLedgerEntries.Add(
            new EscrowLedgerEntry(
                Guid.NewGuid(),
                account.Id,
                hold.Id,
                LedgerTransactionType.Deposit,
                breakdown.GrossAmount,
                runningBalance,
                "MilestoneFunding",
                milestone.Id,
                paymentTransaction.Id,
                "إيداع تمويل المرحلة في حساب الضمان.",
                actorUserId,
                correlationId,
                now));

        MilestoneTransitionGuard.EnsureCanTransition(
            milestone.Status,
            MilestoneStatus.FundedInProgress);
        var previousStatus = milestone.Status;
        milestone.Status = MilestoneStatus.FundedInProgress;
        milestone.FundedAt = now;
        milestone.UpdatedAt = now;
        AddHistory(
            milestone,
            previousStatus,
            MilestoneStatus.FundedInProgress,
            ContractPaymentEventTypes.MilestoneFunded,
            actorUserId,
            "تم تمويل المرحلة وإنشاء حجز الضمان بنجاح.",
            correlationId,
            now);
        await EnqueueMilestoneEventAsync(
            ContractPaymentEventTypes.MilestoneFunded,
            milestone.Id,
            correlationId,
            cancellationToken);

        var response = MapPayment(hold);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            dbContext.ChangeTracker.Clear();
            throw new BusinessException(
                "نجحت عملية الدفع لدى المزود، لكن تعذر توثيق التمويل. تم إيقاف إعادة الخصم وستتم مراجعة العملية تلقائيًا.",
                exception);
        }

        if (reservationId.HasValue)
        {
            await idempotencyService.CompleteAsync(
                reservationId.Value,
                200,
                response,
                hold.Id,
                cancellationToken);
        }

        return response;
    }

    private async Task<FundingOperationDto> FailFundingAsync(
        Milestone milestone,
        PaymentTransaction paymentTransaction,
        Guid reservationId,
        Guid actorUserId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var now = UtcNow;
        paymentTransaction.Status = PaymentTransactionStatus.Failed;
        paymentTransaction.FailureReason =
            "رفض مزود الدفع عملية تمويل المرحلة.";
        paymentTransaction.ProcessedAt = now;
        paymentTransaction.UpdatedAt = now;

        MilestoneTransitionGuard.EnsureCanTransition(
            milestone.Status,
            MilestoneStatus.AwaitingFunding);
        var previousStatus = milestone.Status;
        milestone.Status = MilestoneStatus.AwaitingFunding;
        milestone.UpdatedAt = now;
        AddHistory(
            milestone,
            previousStatus,
            MilestoneStatus.AwaitingFunding,
            ContractPaymentEventTypes.MilestoneFundingFailed,
            actorUserId,
            "رفض مزود الدفع عملية التمويل ولم يتم إنشاء حجز ضمان.",
            correlationId,
            now);
        await EnqueueMilestoneEventAsync(
            ContractPaymentEventTypes.MilestoneFundingFailed,
            milestone.Id,
            correlationId,
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        const string message =
            "رفض مزود الدفع عملية تمويل المرحلة. لم يتم خصم المبلغ ويمكن المحاولة مرة أخرى بمفتاح طلب جديد.";
        await FailReservationAsync(
            reservationId,
            paymentTransaction.Id,
            message,
            cancellationToken);
        throw new BusinessException(message);
    }

    private async Task<FundingOperationDto> KeepUnknownAndThrowAsync(
        PaymentTransaction paymentTransaction,
        CancellationToken cancellationToken)
    {
        await KeepProcessingForReconciliationAsync(
            paymentTransaction,
            "نتيجة عملية الدفع غير مؤكدة وتحتاج إلى مراجعة.",
            cancellationToken);
        throw new BusinessException(
            "نتيجة تمويل المرحلة غير مؤكدة. لا تحاول الدفع مرة أخرى، وستتم مراجعة العملية تلقائيًا.");
    }

    private async Task KeepProcessingForReconciliationAsync(
        PaymentTransaction paymentTransaction,
        string reason,
        CancellationToken cancellationToken)
    {
        paymentTransaction.Status =
            PaymentTransactionStatus.Processing;
        paymentTransaction.FailureReason = reason;
        paymentTransaction.UpdatedAt = UtcNow;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();
        }
    }

    private async Task<FundingOperationDto> KeepPendingFundingAsync(
        Milestone milestone,
        PaymentTransaction paymentTransaction,
        ProviderResult providerResult,
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(providerResult.ProviderTransactionId))
        {
            return await KeepUnknownAndThrowAsync(
                paymentTransaction,
                cancellationToken);
        }

        ApplyProviderResult(paymentTransaction, providerResult);
        paymentTransaction.Status = PaymentTransactionStatus.Processing;
        paymentTransaction.FailureReason = null;
        paymentTransaction.UpdatedAt = UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new FundingOperationDto(
            paymentTransaction.Id,
            milestone.Id,
            providerResult.Outcome.ToString(),
            providerResult.ClientAction?.Type.ToString(),
            providerResult.ClientAction?.ClientSecret,
            providerResult.ClientAction?.RedirectUrl,
            null,
            UtcNow);
        return response;
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
                NumberStyles.None,
                CultureInfo.InvariantCulture,
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

        var secret = paymentProviderOptions.Value.WebhookSecret;
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
        if (request.Status != paymentTransaction.Status)
        {
            throw new BusinessException(
                "يتعارض إشعار مزود الدفع مع النتيجة النهائية المسجلة للمعاملة.");
        }

        dbContext.PaymentWebhookEvents.Add(
            new PaymentWebhookEvent(
                Guid.NewGuid(),
                request.EventId,
                paymentTransaction.Id,
                UtcNow));
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();
            if (!await WebhookEventExistsAsync(
                    request.EventId,
                    cancellationToken))
            {
                throw new BusinessException(
                    "تعذر تسجيل إشعار مزود الدفع المكرر.");
            }
        }

        return new PaymentActionResultDto(
            paymentTransaction.Id,
            "Duplicate",
            UtcNow);
    }

    public async Task<PaymentActionResultDto> FinalizeFailedExternalResultAsync(
        Milestone milestone,
        PaymentTransaction paymentTransaction,
        string? providerTransactionId,
        Guid? reservationId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var now = UtcNow;
        paymentTransaction.ProviderTransactionId =
            providerTransactionId;
        paymentTransaction.Status = PaymentTransactionStatus.Failed;
        paymentTransaction.FailureReason =
            "أكد مزود الدفع فشل عملية تمويل المرحلة.";
        paymentTransaction.ProcessedAt = now;
        paymentTransaction.UpdatedAt = now;

        MilestoneTransitionGuard.EnsureCanTransition(
            milestone.Status,
            MilestoneStatus.AwaitingFunding);
        var previousStatus = milestone.Status;
        milestone.Status = MilestoneStatus.AwaitingFunding;
        milestone.UpdatedAt = now;
        AddHistory(
            milestone,
            previousStatus,
            MilestoneStatus.AwaitingFunding,
            ContractPaymentEventTypes.MilestoneFundingFailed,
            null,
            "أكد إشعار مزود الدفع فشل عملية التمويل.",
            correlationId,
            now);
        await EnqueueMilestoneEventAsync(
            ContractPaymentEventTypes.MilestoneFundingFailed,
            milestone.Id,
            correlationId,
            cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();
            throw new BusinessException(
                "تعذر توثيق نتيجة فشل التمويل الواردة من مزود الدفع.");
        }

        if (reservationId.HasValue)
        {
            await FailReservationAsync(
                reservationId.Value,
                paymentTransaction.Id,
                "أكد مزود الدفع فشل عملية تمويل المرحلة.",
                cancellationToken);
        }

        return new PaymentActionResultDto(
            paymentTransaction.Id,
            PaymentTransactionStatus.Failed.ToString(),
            now);
    }

    public async Task<Guid?> FindProcessingFundingReservationIdAsync(
        Guid milestoneId,
        CancellationToken cancellationToken)
    {
        return await dbContext.IdempotencyRecords
            .AsNoTracking()
            .Where(item =>
                item.Operation == FundOperation
                && item.ResourceType == MilestoneResource
                && item.ResourceId == milestoneId
                && item.Status == IdempotencyStatus.Processing)
            .Select(item => (Guid?)item.Id)
            .SingleOrDefaultAsync(cancellationToken);
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

    private async Task EnsureFundingAllowedAsync(
        Milestone milestone,
        ContractStatus contractStatus,
        CancellationToken cancellationToken)
    {
        if (contractStatus != ContractStatus.Active)
        {
            throw new BusinessException(
                "يجب أن يكون العقد نشطًا قبل تمويل أي مرحلة.");
        }

        if (milestone.Status != MilestoneStatus.AwaitingFunding)
        {
            throw new BusinessException(
                "المرحلة ليست جاهزة للتمويل في حالتها الحالية.");
        }

        if (!milestone.AcceptedByClientAt.HasValue
            || !milestone.AcceptedByLawyerAt.HasValue)
        {
            throw new BusinessException(
                "يجب أن يوافق العميل والمحامي على المرحلة قبل تمويلها.");
        }

        if (!milestone.ReadyForFundingAt.HasValue)
        {
            throw new BusinessException(
                "يجب أن يجهز المحامي المرحلة للتمويل قبل تنفيذ الدفع.");
        }

        var hasUnsettledEarlierMilestone =
            await dbContext.Milestones.AnyAsync(
                item =>
                    item.ContractId == milestone.ContractId
                    && item.OrderNumber < milestone.OrderNumber
                    && item.Status != MilestoneStatus.Released
                    && item.Status != MilestoneStatus.Refunded
                    && item.Status != MilestoneStatus.Cancelled,
                cancellationToken);
        if (hasUnsettledEarlierMilestone)
        {
            throw new BusinessException(
                "يجب تسوية جميع المراحل السابقة قبل تمويل هذه المرحلة.");
        }

        var hasOtherActiveMilestone =
            await dbContext.Milestones.AnyAsync(
                item =>
                    item.ContractId == milestone.ContractId
                    && item.Id != milestone.Id
                    && (item.Status
                            == MilestoneStatus.FundingProcessing
                        || item.Status
                            == MilestoneStatus.FundedInProgress
                        || item.Status == MilestoneStatus.Submitted
                        || item.Status == MilestoneStatus.AcceptedHold
                        || item.Status == MilestoneStatus.Disputed),
                cancellationToken);
        var hasOtherUnsettledHold =
            await dbContext.EscrowHolds.AnyAsync(
                hold =>
                    hold.ContractId == milestone.ContractId
                    && hold.MilestoneId != milestone.Id
                    && (hold.Status == EscrowHoldStatus.Funded
                        || hold.Status == EscrowHoldStatus.Frozen),
                cancellationToken);
        if (hasOtherActiveMilestone || hasOtherUnsettledHold)
        {
            throw new BusinessException(
                "لا يمكن تمويل مرحلة جديدة قبل حسم المرحلة الممولة أو المعلقة حاليًا.");
        }

        if (await dbContext.EscrowHolds.AnyAsync(
                hold => hold.MilestoneId == milestone.Id,
                cancellationToken))
        {
            throw new ConflictException(
                "تم إنشاء حجز ضمان لهذه المرحلة مسبقًا.");
        }
    }

    private async Task<FundingOperationDto> ReplayAsync(
        IdempotencyReservation reservation,
        CancellationToken cancellationToken)
    {
        if (reservation.Status == IdempotencyStatus.Failed)
        {
            var failure = DeserializeFailure(
                reservation.ResponseBody);
            throw new BusinessException(
                failure?.Message
                ?? "فشلت محاولة تمويل المرحلة السابقة المرتبطة بمفتاح الطلب.");
        }

        if (!string.IsNullOrWhiteSpace(reservation.ResponseBody))
        {
            FundingOperationDto? operationResponse = null;
            try
            {
                operationResponse =
                    JsonSerializer.Deserialize<FundingOperationDto>(
                        reservation.ResponseBody,
                        SerializerOptions);
            }
            catch (JsonException)
            {
                // Completed records from versions before the current provider flow
                // contain PaymentDto directly and are handled below.
            }
            if (operationResponse is not null)
            {
                return operationResponse;
            }

            var paymentResponse = JsonSerializer.Deserialize<PaymentDto>(
                reservation.ResponseBody,
                SerializerOptions);
            if (paymentResponse is not null)
            {
                var transactionId = await dbContext.EscrowHolds
                    .AsNoTracking()
                    .Where(item => item.Id == paymentResponse.Id)
                    .Select(item => (Guid?)item.ProviderDepositTransactionId)
                    .SingleOrDefaultAsync(cancellationToken)
                    ?? Guid.Empty;
                return new FundingOperationDto(
                    transactionId,
                    paymentResponse.MilestoneId,
                    ProviderOperationOutcome.Succeeded.ToString(),
                    null,
                    null,
                    null,
                    paymentResponse,
                    paymentResponse.SettledAt ?? UtcNow);
            }
        }

        if (reservation.ResultReferenceId.HasValue)
        {
            var hold = await dbContext.EscrowHolds
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.Id
                            == reservation.ResultReferenceId.Value,
                    cancellationToken);
            if (hold is not null)
            {
                var payment = MapPayment(hold);
                return new FundingOperationDto(
                    hold.ProviderDepositTransactionId,
                    hold.MilestoneId,
                    ProviderOperationOutcome.Succeeded.ToString(),
                    null,
                    null,
                    null,
                    payment,
                    hold.CreatedAt);
            }
        }

        throw new BusinessException(
            "تعذر استعادة نتيجة عملية التمويل السابقة المرتبطة بمفتاح الطلب.");
    }

    private async Task FailReservationAsync(
        Guid reservationId,
        Guid? paymentTransactionId,
        string message,
        CancellationToken cancellationToken)
    {
        await idempotencyService.FailAsync(
            reservationId,
            409,
            new PaymentFailureResponse(message),
            paymentTransactionId,
            cancellationToken);
    }

    private async Task<Contract> GetAuthorizedPaymentContractAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        if (contractId == Guid.Empty)
        {
            throw new BusinessException(
                "معرّف العقد مطلوب لعرض بيانات الدفع.");
        }

        var actorUserId = GetActorUserId();
        var contract = await dbContext.Contracts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == contractId,
                cancellationToken)
            ?? throw new NotFoundException(
                "العقد المطلوب غير موجود.");
        if (contract.ClientUserId == actorUserId
            || contract.LawyerUserId == actorUserId)
        {
            return contract;
        }

        var eligibility =
            await userEligibilityService.FindEligibilityAsync(
                actorUserId,
                cancellationToken);
        if (eligibility is null
            || eligibility.UserId != actorUserId
            || !eligibility.IsActive
            || (!eligibility.CanActAsFinanceAdministrator
                && !eligibility.CanActAsSuperAdministrator))
        {
            throw new ForbiddenAccessException(
                "غير مصرح لك بالاطلاع على بيانات الدفع لهذا العقد.");
        }

        return contract;
    }

    private async Task EnsureFinanceOperatorAsync(
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var eligibility =
            await userEligibilityService.FindEligibilityAsync(
                actorUserId,
                cancellationToken);
        if (eligibility is null
            || eligibility.UserId != actorUserId
            || !eligibility.IsActive
            || (!eligibility.CanActAsFinanceAdministrator
                && !eligibility.CanActAsSuperAdministrator))
        {
            throw new ForbiddenAccessException(
                "إعادة محاولة معاملات الدفع متاحة لمسؤولي العمليات المالية فقط.");
        }
    }

    private void AddHistory(
        Milestone milestone,
        MilestoneStatus previousStatus,
        MilestoneStatus newStatus,
        string trigger,
        Guid? actorUserId,
        string reason,
        Guid correlationId,
        DateTime occurredAt)
    {
        MilestoneTransitionGuard.EnsureCanTransition(
            previousStatus,
            newStatus);
        dbContext.MilestoneStateHistories.Add(
            new MilestoneStateHistory(
                Guid.NewGuid(),
                milestone.Id,
                previousStatus,
                newStatus,
                trigger,
                actorUserId,
                reason,
                correlationId,
                occurredAt));
    }

    private async Task EnqueueMilestoneEventAsync(
        string eventType,
        Guid milestoneId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        await outboxWriter.EnqueueAsync(
            new OutboxEvent(
                eventType,
                1,
                new ContractPaymentAggregateEventPayload(
                    milestoneId),
                "Milestone",
                milestoneId,
                correlationId),
            cancellationToken);
    }

    private Guid GetActorUserId()
    {
        if (!currentUserService.IsAuthenticated
            || !currentUserService.UserId.HasValue
            || currentUserService.UserId.Value == Guid.Empty)
        {
            throw new AuthenticationException(
                "يجب تسجيل الدخول للوصول إلى خدمات الدفع.");
        }

        return currentUserService.UserId.Value;
    }

    private static string RequireIdempotencyKey(
        string? idempotencyKey)
    {
        var key = idempotencyKey?.Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new BusinessException(
                "مفتاح Idempotency-Key مطلوب لتنفيذ عملية التمويل بأمان.");
        }

        if (key.Length > IdempotencyHeader.MaximumLength)
        {
            throw new BusinessException(
                $"مفتاح Idempotency-Key يجب ألا يتجاوز {IdempotencyHeader.MaximumLength} حرف.");
        }

        return key;
    }

    private static string CreateProviderIdempotencyKey(
        Guid actorUserId,
        Guid milestoneId,
        string idempotencyKey)
    {
        var value =
            $"{FundOperation}:{actorUserId:N}:{milestoneId:N}:{idempotencyKey}";
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private static string CreateRetryProviderIdempotencyKey(
        string originalProviderIdempotencyKey,
        string retryIdempotencyKey)
    {
        var value =
            $"{RetryOperation}:{originalProviderIdempotencyKey}:{retryIdempotencyKey}";
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private static bool ProviderResultMatches(
        ProviderResult result,
        PaymentProviderRequest request)
    {
        return result.Amount == request.Amount
            && string.Equals(
                result.Currency,
                request.Currency,
                StringComparison.Ordinal)
            && result.BusinessId == request.BusinessId
            && string.Equals(
                result.ProviderIdempotencyKey,
                request.ProviderIdempotencyKey,
                StringComparison.Ordinal)
            && result.CorrelationId == request.CorrelationId;
    }

    private static bool ReconciliationResultMatches(
        ProviderResult result,
        PaymentTransaction paymentTransaction,
        Guid milestoneId)
    {
        return result.Amount == paymentTransaction.Amount
            && string.Equals(
                result.Currency,
                paymentTransaction.Currency,
                StringComparison.Ordinal)
            && result.BusinessId == milestoneId
            && string.Equals(
                result.ProviderIdempotencyKey,
                paymentTransaction.IdempotencyKey,
                StringComparison.Ordinal);
    }

    private static PaymentDto MapPayment(EscrowHold hold)
    {
        return new PaymentDto(
            hold.Id,
            hold.MilestoneId,
            hold.GrossAmount,
            hold.PlatformFeeAmount,
            hold.NetAmount,
            "EGP",
            hold.Status,
            hold.HoldExpiresAt,
            hold.SettledAt);
    }

    private async Task<string> GetClientCustomerReferenceAsync(
        Guid clientUserId,
        CancellationToken cancellationToken)
    {
        return await dbContext.ClientPaymentCustomers
            .AsNoTracking()
            .Where(item => item.ClientUserId == clientUserId
                && item.ProviderCode == paymentProviderOptions.Value.ProviderCode)
            .Select(item => item.ProviderCustomerId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? string.Empty;
    }

    private async Task<string> ResolveCustomerForPaymentMethodAsync(
        Guid clientUserId,
        string paymentMethodReference,
        CancellationToken cancellationToken)
    {
        var customerReference = await GetClientCustomerReferenceAsync(
            clientUserId,
            cancellationToken);
        if (string.IsNullOrWhiteSpace(customerReference)
            || paymentProvider is not IClientPaymentMethodProvider
                paymentMethodProvider)
        {
            return string.Empty;
        }

        var savedMethods = await paymentMethodProvider.ListPaymentMethodsAsync(
            customerReference,
            cancellationToken);
        return savedMethods.Any(item => string.Equals(
            item.ProviderPaymentMethodId,
            paymentMethodReference,
            StringComparison.Ordinal))
            ? customerReference
            : string.Empty;
    }

    private static FundingOperationDto CompletedFunding(
        PaymentDto payment,
        PaymentTransaction transaction)
        => new(
            transaction.Id,
            payment.MilestoneId,
            ProviderOperationOutcome.Succeeded.ToString(),
            null,
            null,
            null,
            payment,
            transaction.ProcessedAt ?? transaction.UpdatedAt);

    private static void ApplyProviderResult(
        PaymentTransaction transaction,
        ProviderResult result)
    {
        transaction.ProviderTransactionId = result.ProviderTransactionId;
        transaction.ProviderRelatedTransactionId =
            result.RelatedProviderTransactionId;
        transaction.ProviderStatus = result.ProviderStatus;
        transaction.ProviderObjectType = result.ProviderObjectType;
        transaction.ProviderAmountMinor = result.ProviderMoney?.AmountMinor;
        transaction.ProviderCurrency = result.ProviderMoney?.Currency;
    }

    private static PaymentFailureResponse? DeserializeFailure(
        string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<PaymentFailureResponse>(
                responseBody,
                SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private DateTime UtcNow =>
        timeProvider.GetUtcNow().UtcDateTime;

    private sealed record PaymentFailureResponse(string Message);
}
