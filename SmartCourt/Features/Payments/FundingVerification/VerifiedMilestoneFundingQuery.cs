using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Payments.FundingVerification;

internal static class VerifiedMilestoneFundingQuery
{
    internal static IQueryable<VerifiedMilestoneFunding> Create(
        IQueryable<Milestone> milestones,
        IQueryable<EscrowAccount> escrowAccounts,
        IQueryable<EscrowHold> escrowHolds,
        IQueryable<PaymentTransaction> paymentTransactions,
        Guid milestoneId,
        FundingVerificationOperation operation)
    {
        if (milestoneId == Guid.Empty)
        {
            throw new BusinessException(
                "معرّف المرحلة مطلوب للتحقق من التمويل.");
        }

        var requiredHoldStatus = operation switch
        {
            FundingVerificationOperation.Submission
                => EscrowHoldStatus.Funded,
            FundingVerificationOperation.ManualAcceptance
                => EscrowHoldStatus.Funded,
            FundingVerificationOperation.AutomaticAcceptance
                => EscrowHoldStatus.Funded,
            FundingVerificationOperation.DisputeOpening
                => EscrowHoldStatus.Funded,
            _ => throw new BusinessException(
                "نوع عملية التحقق من تمويل المرحلة غير صالح.")
        };

        return
            from milestone in milestones
            where milestone.Id == milestoneId
                && milestone.FundedAt.HasValue
                && escrowHolds.Count(
                    hold => hold.MilestoneId == milestone.Id) == 1
            join hold in escrowHolds
                on milestone.Id equals hold.MilestoneId
            join account in escrowAccounts
                on hold.EscrowAccountId equals account.Id
            join transaction in paymentTransactions
                on hold.ProviderDepositTransactionId equals transaction.Id
            where hold.ContractId == milestone.ContractId
                && account.ContractId == milestone.ContractId
                && account.Currency == EntityGuard.CurrencyEgp
                && hold.Status == requiredHoldStatus
                && hold.GrossAmount == milestone.Amount
                && transaction.ContractId == milestone.ContractId
                && transaction.MilestoneId == milestone.Id
                && transaction.EscrowHoldId == hold.Id
                && transaction.OperationType == PaymentOperationType.Deposit
                && transaction.Status == PaymentTransactionStatus.Completed
                && transaction.Amount == milestone.Amount
                && transaction.Amount == hold.GrossAmount
                && transaction.Currency == account.Currency
                && transaction.Currency == EntityGuard.CurrencyEgp
            select new VerifiedMilestoneFunding(
                milestone.Id,
                milestone.ContractId,
                account.Id,
                hold.Id,
                transaction.Id,
                hold.GrossAmount,
                transaction.Currency,
                milestone.FundedAt.GetValueOrDefault());
    }
}
