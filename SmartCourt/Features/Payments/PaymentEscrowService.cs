using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Milestones.Domain;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.Settlement;
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
    IPaymentProvider paymentProvider,
    IPaymentReconciliationProvider reconciliationProvider,
    IIdempotencyService idempotencyService,
    IOutboxWriter outboxWriter,
    IOptions<PaymentProviderOptions> paymentProviderOptions,
    ILogger<PaymentEscrowService> logger,
    TimeProvider timeProvider) : IPaymentEscrowService
{
    private const string FundOperation = "FundMilestone";
    private const string MilestoneResource = "Milestone";
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<PaymentDto> FundAsync(
        Guid milestoneId,
        FundMilestoneRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
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
            request.PaymentMethodReference);

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
                await CompleteFundingAsync(
                    milestone,
                    contract.LawyerUserId,
                    paymentTransaction,
                    providerResult,
                    reservation.RecordId,
                    actorUserId,
                    correlationId,
                    cancellationToken),
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
            _ => throw new BusinessException(
                "أعاد مزود الدفع نتيجة غير صالحة لعملية تمويل المرحلة.")
        };
    }

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
            await FindProcessingFundingReservationIdAsync(
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
                await CompleteFundingAsync(
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

        return await FinalizeFailedExternalResultAsync(
            milestone,
            paymentTransaction,
            request.ProviderTransactionId,
            reservationId,
            correlationId,
            cancellationToken);
    }

    public async Task<JobExecutionResult> ReconcileProviderTransactionAsync(
        Guid paymentTransactionId,
        CancellationToken cancellationToken)
    {
        if (paymentTransactionId == Guid.Empty)
        {
            throw new BusinessException(
                "معرّف معاملة الدفع مطلوب لإجراء المطابقة.");
        }

        var paymentTransaction = await dbContext.PaymentTransactions
            .SingleOrDefaultAsync(
                item => item.Id == paymentTransactionId,
                cancellationToken);
        if (paymentTransaction is null)
        {
            return JobExecutionResult.NoOp(
                "PaymentTransactionNotFound");
        }

        if (paymentTransaction.Status
            != PaymentTransactionStatus.Processing)
        {
            return JobExecutionResult.NoOp(
                "PaymentTransactionAlreadyFinal");
        }

        if (paymentTransaction.OperationType
                != PaymentOperationType.Deposit
            || !paymentTransaction.MilestoneId.HasValue)
        {
            return JobExecutionResult.NoOp(
                "PaymentOperationNotSupported");
        }

        var milestone = await dbContext.Milestones
            .SingleOrDefaultAsync(
                item =>
                    item.Id
                        == paymentTransaction.MilestoneId.Value,
                cancellationToken);
        if (milestone is null
            || milestone.Status
                != MilestoneStatus.FundingProcessing)
        {
            return JobExecutionResult.NoOp(
                "MilestoneNoLongerAwaitingReconciliation");
        }

        var correlationId = Guid.NewGuid();
        var result =
            await reconciliationProvider.GetDepositStatusAsync(
                new ProviderDepositStatusRequest(
                    paymentTransaction.Amount,
                    paymentTransaction.Currency,
                    milestone.Id,
                    paymentTransaction.IdempotencyKey,
                    correlationId),
                cancellationToken);
        if (result is null
            || result.Outcome == ProviderOperationOutcome.Unknown)
        {
            return JobExecutionResult.NoOp(
                "ProviderOutcomeStillUnknown");
        }

        if (!ReconciliationResultMatches(
                result,
                paymentTransaction,
                milestone.Id))
        {
            logger.LogWarning(
                "Rejected provider reconciliation result for transaction {PaymentTransactionId}: financial facts do not match.",
                paymentTransaction.Id);
            throw new BusinessException(
                "نتيجة مطابقة مزود الدفع لا تطابق بيانات معاملة التمويل الأصلية.");
        }

        var contract = await dbContext.Contracts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == paymentTransaction.ContractId,
                cancellationToken)
            ?? throw new BusinessException(
                "العقد المرتبط بمعاملة الدفع غير موجود.");
        var reservationId =
            await FindProcessingFundingReservationIdAsync(
                milestone.Id,
                cancellationToken);

        if (result.Outcome == ProviderOperationOutcome.Succeeded)
        {
            await CompleteFundingAsync(
                milestone,
                contract.LawyerUserId,
                paymentTransaction,
                result,
                reservationId,
                null,
                correlationId,
                cancellationToken);
        }
        else
        {
            await FinalizeFailedExternalResultAsync(
                milestone,
                paymentTransaction,
                result.ProviderTransactionId
                    ?? $"reconciled-failed-{paymentTransaction.Id:N}",
                reservationId,
                correlationId,
                cancellationToken);
        }

        return JobExecutionResult.Completed(
            "ProviderTransactionReconciled");
    }

    private async Task<PaymentDto> CompleteFundingAsync(
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

    private async Task<PaymentDto> FailFundingAsync(
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

    private async Task<PaymentDto> KeepUnknownAndThrowAsync(
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

    private async Task<PaymentActionResultDto> FinalizeFailedExternalResultAsync(
        Milestone milestone,
        PaymentTransaction paymentTransaction,
        string providerTransactionId,
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

    private async Task<Guid?> FindProcessingFundingReservationIdAsync(
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

    private async Task<PaymentDto> ReplayAsync(
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
            var response = JsonSerializer.Deserialize<PaymentDto>(
                reservation.ResponseBody,
                SerializerOptions);
            if (response is not null)
            {
                return response;
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
                return MapPayment(hold);
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
                "يجب تسجيل الدخول لإتمام عملية التمويل.");
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

    private static bool ProviderResultMatches(
        ProviderResult result,
        ProviderDepositRequest request)
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
