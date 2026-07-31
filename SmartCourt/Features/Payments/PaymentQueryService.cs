using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Payments;

public sealed class PaymentQueryService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IContractUserEligibilityService userEligibilityService) : IPaymentQueryService
{
    public async Task<PaymentHistoryDto> GetContractPaymentsAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        var contract = await GetAuthorizedPaymentContractAsync(
            contractId,
            cancellationToken);

        var holds = await dbContext.EscrowHolds
            .AsNoTracking()
            .Where(hold => hold.ContractId == contract.Id)
            .OrderBy(hold => hold.FundedAt)
            .ThenBy(hold => hold.Id)
            .ToListAsync(cancellationToken);

        var attempts = await dbContext.PaymentTransactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.ContractId == contract.Id)
            .OrderByDescending(transaction => transaction.CreatedAt)
            .ThenBy(transaction => transaction.Id)
            .Select(transaction => new PaymentAttemptDto(
                transaction.Id,
                transaction.MilestoneId,
                transaction.OperationType,
                transaction.Status,
                transaction.Amount,
                transaction.Currency,
                transaction.ProviderName,
                transaction.CreatedAt,
                transaction.ProcessedAt))
            .ToListAsync(cancellationToken);

        var ledgerEntries = await dbContext.EscrowLedgerEntries
            .AsNoTracking()
            .Where(entry => dbContext.EscrowAccounts.Any(account =>
                account.Id == entry.EscrowAccountId
                && account.ContractId == contract.Id))
            .OrderBy(entry => entry.CreatedAt)
            .ThenBy(entry => entry.Id)
            .Select(entry => new EscrowLedgerEntryDto(
                entry.Id,
                entry.EscrowHoldId,
                entry.TransactionType,
                entry.Amount,
                entry.RunningBalance,
                entry.Currency,
                entry.Description,
                entry.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PaymentHistoryDto(
            holds.Select(MapPayment).ToArray(),
            attempts,
            ledgerEntries);
    }

    public async Task<PaymentDto> GetMilestonePaymentAsync(
        Guid milestoneId,
        CancellationToken cancellationToken)
    {
        if (milestoneId == Guid.Empty)
        {
            throw new BusinessException(
                "معرّف المرحلة مطلوب لعرض بيانات الدفع.");
        }

        var milestone = await dbContext.Milestones
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == milestoneId,
                cancellationToken)
            ?? throw new NotFoundException(
                "المرحلة المطلوبة غير موجودة.");

        await GetAuthorizedPaymentContractAsync(
            milestone.ContractId,
            cancellationToken);

        var hold = await dbContext.EscrowHolds
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.MilestoneId == milestone.Id,
                cancellationToken)
            ?? throw new NotFoundException(
                "لم يتم إنشاء حجز دفع لهذه المرحلة بعد.");

        return MapPayment(hold);
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

        var actorUserId = currentUserService.RequireUserId(
            "يجب تسجيل الدخول للوصول إلى خدمات الدفع.");

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
}
