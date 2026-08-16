using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts.Domain;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Disputes.Domain;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Milestones.Domain;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Domain;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.FundingVerification;
using SmartCourt.Features.Payments.Settlement;
using Xunit;

namespace SmartCourt.Tests.Features.Payments.Domain;

public sealed class ContractAndPaymentInvariantTests
{
    private static readonly DateTime UtcNow =
        new(2026, 7, 27, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ContractTransitionGuard_AllowsOnlyDocumentedTransitions()
    {
        HashSet<(ContractStatus, ContractStatus)> allowed =
        [
            (ContractStatus.Draft, ContractStatus.Active),
            (ContractStatus.Draft, ContractStatus.Terminated),
            (ContractStatus.Active, ContractStatus.SuspendedByDispute),
            (ContractStatus.Active, ContractStatus.Completed),
            (ContractStatus.Active, ContractStatus.Terminated),
            (ContractStatus.SuspendedByDispute, ContractStatus.Active),
            (ContractStatus.SuspendedByDispute, ContractStatus.Completed),
            (ContractStatus.SuspendedByDispute, ContractStatus.Terminated)
        ];

        AssertTransitionMatrix(
            Enum.GetValues<ContractStatus>(),
            allowed,
            ContractTransitionGuard.EnsureCanTransition);
    }

    [Fact]
    public void MilestoneTransitionGuard_AllowsOnlyDocumentedTransitions()
    {
        HashSet<(MilestoneStatus, MilestoneStatus)> allowed =
        [
            (MilestoneStatus.Draft, MilestoneStatus.AwaitingFunding),
            (MilestoneStatus.Draft, MilestoneStatus.Cancelled),
            (MilestoneStatus.AwaitingFunding, MilestoneStatus.FundingProcessing),
            (MilestoneStatus.AwaitingFunding, MilestoneStatus.Cancelled),
            (MilestoneStatus.FundingProcessing, MilestoneStatus.FundedInProgress),
            (MilestoneStatus.FundingProcessing, MilestoneStatus.ReleasePending),
            (MilestoneStatus.FundingProcessing, MilestoneStatus.AwaitingFunding),
            (MilestoneStatus.FundingProcessing, MilestoneStatus.Cancelled),
            (MilestoneStatus.FundedInProgress, MilestoneStatus.Submitted),
            (MilestoneStatus.FundedInProgress, MilestoneStatus.Cancelled),
            (MilestoneStatus.FundedInProgress, MilestoneStatus.Refunded),
            (MilestoneStatus.Submitted, MilestoneStatus.FundedInProgress),
            (MilestoneStatus.Submitted, MilestoneStatus.AcceptedHold),
            (MilestoneStatus.Submitted, MilestoneStatus.Refunded),
            (MilestoneStatus.AcceptedHold, MilestoneStatus.Disputed),
            (MilestoneStatus.AcceptedHold, MilestoneStatus.Released),
            (MilestoneStatus.AcceptedHold, MilestoneStatus.Refunded),
            (MilestoneStatus.Disputed, MilestoneStatus.Released),
            (MilestoneStatus.Disputed, MilestoneStatus.Refunded),
            (MilestoneStatus.ReleasePending, MilestoneStatus.Released)
        ];

        AssertTransitionMatrix(
            Enum.GetValues<MilestoneStatus>(),
            allowed,
            MilestoneTransitionGuard.EnsureCanTransition);
    }

    [Fact]
    public void EscrowHoldTransitionGuard_AllowsOnlyDocumentedTransitions()
    {
        HashSet<(EscrowHoldStatus, EscrowHoldStatus)> allowed =
        [
            (EscrowHoldStatus.Funded, EscrowHoldStatus.Frozen),
            (EscrowHoldStatus.Funded, EscrowHoldStatus.Released),
            (EscrowHoldStatus.Funded, EscrowHoldStatus.Refunded),
            (EscrowHoldStatus.Frozen, EscrowHoldStatus.Released),
            (EscrowHoldStatus.Frozen, EscrowHoldStatus.Refunded)
        ];

        AssertTransitionMatrix(
            Enum.GetValues<EscrowHoldStatus>(),
            allowed,
            EscrowHoldTransitionGuard.EnsureCanTransition);
    }

    [Fact]
    public void DisputeTransitionGuard_AllowsOnlyDocumentedTransitions()
    {
        HashSet<(DisputeStatus, DisputeStatus)> allowed =
        [
            (DisputeStatus.Open, DisputeStatus.Assigned),
            (DisputeStatus.Assigned, DisputeStatus.UnderReview),
            (DisputeStatus.UnderReview, DisputeStatus.Resolved),
            (DisputeStatus.Resolved, DisputeStatus.Closed)
        ];

        AssertTransitionMatrix(
            Enum.GetValues<DisputeStatus>(),
            allowed,
            DisputeTransitionGuard.EnsureCanTransition);
    }

    [Fact]
    public void ChangeRequestTransitionGuard_AllowsOnlyDocumentedTransitions()
    {
        HashSet<(ChangeRequestStatus, ChangeRequestStatus)> allowed =
        [
            (ChangeRequestStatus.Pending, ChangeRequestStatus.Approved),
            (ChangeRequestStatus.Pending, ChangeRequestStatus.Rejected),
            (ChangeRequestStatus.Pending, ChangeRequestStatus.Cancelled)
        ];

        AssertTransitionMatrix(
            Enum.GetValues<ChangeRequestStatus>(),
            allowed,
            ChangeRequestTransitionGuard.EnsureCanTransition);
    }

    [Fact]
    public void TransitionGuards_RejectUndefinedPersistedStates()
    {
        Assert.Throws<BusinessException>(() =>
            ContractTransitionGuard.EnsureCanTransition(
                (ContractStatus)999,
                ContractStatus.Active));
        Assert.Throws<BusinessException>(() =>
            MilestoneTransitionGuard.EnsureCanTransition(
                MilestoneStatus.Draft,
                (MilestoneStatus)999));
        Assert.Throws<BusinessException>(() =>
            EscrowHoldTransitionGuard.EnsureCanTransition(
                (EscrowHoldStatus)999,
                EscrowHoldStatus.Frozen));
        Assert.Throws<BusinessException>(() =>
            DisputeTransitionGuard.EnsureCanTransition(
                DisputeStatus.Open,
                (DisputeStatus)999));
        Assert.Throws<BusinessException>(() =>
            ChangeRequestTransitionGuard.EnsureCanTransition(
                (ChangeRequestStatus)999,
                ChangeRequestStatus.Approved));
    }

    [Fact]
    public void ContractStateHistoryFactory_RequiresCompleteAuditContext()
    {
        var history = ContractStateHistoryFactory.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            ContractStatus.Draft,
            ContractStatus.Active,
            "BothPartiesAccepted",
            Guid.NewGuid(),
            "Current terms accepted.",
            Guid.NewGuid(),
            UtcNow);

        Assert.Equal(ContractStatus.Draft, history.PreviousStatus);
        Assert.Equal(ContractStatus.Active, history.NewStatus);
        Assert.Equal("BothPartiesAccepted", history.Trigger);
        Assert.NotEqual(Guid.Empty, history.ActorUserId);
        Assert.Equal("Current terms accepted.", history.Reason);
        Assert.Equal(TimeSpan.Zero, history.CreatedAt.Offset);

        Assert.Throws<BusinessException>(() =>
            ContractStateHistoryFactory.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                ContractStatus.Draft,
                ContractStatus.Active,
                "BothPartiesAccepted",
                Guid.NewGuid(),
                " ",
                Guid.NewGuid(),
                UtcNow));
    }

    [Fact]
    public void MilestoneStateHistoryFactory_RequiresUtcAndLegalTransition()
    {
        var history = MilestoneStateHistoryFactory.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            MilestoneStatus.Draft,
            MilestoneStatus.AwaitingFunding,
            "MilestoneApproved",
            Guid.NewGuid(),
            "Both participants approved.",
            Guid.NewGuid(),
            UtcNow);

        Assert.Equal(MilestoneStatus.Draft, history.PreviousStatus);
        Assert.Equal(MilestoneStatus.AwaitingFunding, history.NewStatus);
        Assert.Equal(TimeSpan.Zero, history.CreatedAt.Offset);

        Assert.Throws<BusinessException>(() =>
            MilestoneStateHistoryFactory.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                MilestoneStatus.Draft,
                MilestoneStatus.Released,
                "Invalid",
                Guid.NewGuid(),
                "Invalid transition.",
                Guid.NewGuid(),
                UtcNow));

        Assert.Throws<BusinessException>(() =>
            MilestoneStateHistoryFactory.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                MilestoneStatus.Draft,
                MilestoneStatus.AwaitingFunding,
                "MilestoneApproved",
                Guid.NewGuid(),
                "Both participants approved.",
                Guid.NewGuid(),
                DateTime.SpecifyKind(UtcNow, DateTimeKind.Local)));
    }

    [Fact]
    public void FundingQuery_ValidChainVerifiesForEveryConsumer()
    {
        foreach (var operation
            in Enum.GetValues<FundingVerificationOperation>())
        {
            var chain = CreateValidFundingChain();

            var result = Query(chain, operation).Single();

            Assert.Equal(chain.Milestone.Id, result.MilestoneId);
            Assert.Equal(chain.Hold.Id, result.EscrowHoldId);
            Assert.Equal(chain.Transaction.Id, result.DepositTransactionId);
            Assert.Equal(chain.Milestone.Amount, result.GrossAmount);
            Assert.Equal("EGP", result.Currency);
            Assert.Equal(chain.Milestone.FundedAt, result.FundedAt);
        }
    }

    [Fact]
    public void FundingQuery_PaymentForMilestoneACannotVerifyMilestoneB()
    {
        var chain = CreateValidFundingChain();
        var milestoneB = CreateMilestone(Guid.NewGuid(), chain.ContractId);
        milestoneB.FundedAt = UtcNow;

        var result = VerifiedMilestoneFundingQuery.Create(
                new[] { chain.Milestone, milestoneB }.AsQueryable(),
                new[] { chain.Account }.AsQueryable(),
                new[] { chain.Hold }.AsQueryable(),
                new[] { chain.Transaction }.AsQueryable(),
                milestoneB.Id,
                FundingVerificationOperation.Submission)
            .SingleOrDefault();

        Assert.Null(result);
    }

    [Fact]
    public void FundingQuery_MismatchReturnsNoVerifiedResult()
    {
        foreach (var mismatch in Enum.GetValues<FundingMismatch>())
        {
            var chain = CreateValidFundingChain();

            switch (mismatch)
            {
                case FundingMismatch.MissingFundedAt:
                    chain.Milestone.FundedAt = null;
                    break;
                case FundingMismatch.MissingHold:
                    chain.Holds.Clear();
                    break;
                case FundingMismatch.MultipleHolds:
                    chain.Holds.Add(CreateHold(
                        Guid.NewGuid(),
                        chain.Account.Id,
                        chain.ContractId,
                        chain.Milestone.Id,
                        Guid.NewGuid()));
                    break;
                case FundingMismatch.MissingDeposit:
                    chain.Transactions.Clear();
                    break;
                case FundingMismatch.NonCompletedDeposit:
                    chain.Transaction.Status =
                        PaymentTransactionStatus.Processing;
                    break;
                case FundingMismatch.MilestoneHoldAmount:
                    chain.Milestone.Amount = 101m;
                    break;
                case FundingMismatch.TransactionAmount:
                    chain.Transaction.Amount = 99m;
                    break;
                case FundingMismatch.TransactionCurrency:
                    chain.Transaction.Currency = "USD";
                    break;
                case FundingMismatch.AccountCurrency:
                    chain.Account.Currency = "USD";
                    chain.Transaction.Currency = "USD";
                    break;
                case FundingMismatch.WrongHoldStatus:
                    chain.Hold.Status = EscrowHoldStatus.Frozen;
                    break;
                case FundingMismatch.WrongHoldReference:
                    chain.Transaction.EscrowHoldId = Guid.NewGuid();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(mismatch),
                        mismatch,
                        null);
            }

            Assert.Empty(Query(
                chain,
                FundingVerificationOperation.Submission));
        }
    }

    [Fact]
    public void FundingQuery_RejectsInvalidInputs()
    {
        var chain = CreateValidFundingChain();

        Assert.Throws<BusinessException>(() =>
            VerifiedMilestoneFundingQuery.Create(
                chain.Milestones.AsQueryable(),
                chain.Accounts.AsQueryable(),
                chain.Holds.AsQueryable(),
                chain.Transactions.AsQueryable(),
                Guid.Empty,
                FundingVerificationOperation.Submission));

        Assert.Throws<BusinessException>(() =>
            Query(chain, (FundingVerificationOperation)999));
    }

    [Theory]
    [InlineData(100, 100, 0, 0, 0)]
    [InlineData(100, 0, 100, 5, 95)]
    [InlineData(100, 40, 60, 3, 57)]
    [InlineData(10.10, 0, 10.10, 0.51, 9.59)]
    public void SettlementCalculator_ReconcilesEveryOutcome(
        double gross,
        double refund,
        double lawyerGross,
        double fee,
        double lawyerNet)
    {
        var result = SettlementCalculator.Calculate(
            (decimal)gross,
            (decimal)refund);

        Assert.Equal((decimal)gross, result.GrossAmount);
        Assert.Equal((decimal)refund, result.ClientRefundAmount);
        Assert.Equal((decimal)lawyerGross, result.LawyerGrossAllocation);
        Assert.Equal((decimal)fee, result.PlatformFeeAmount);
        Assert.Equal((decimal)lawyerNet, result.LawyerNetAmount);
        Assert.Equal(
            result.GrossAmount,
            result.ClientRefundAmount
            + result.LawyerNetAmount
            + result.PlatformFeeAmount);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100, -1)]
    [InlineData(100, 101)]
    [InlineData(100.001, 0)]
    [InlineData(100, 1.001)]
    public void SettlementCalculator_RejectsInvalidAmounts(
        double gross,
        double refund)
    {
        Assert.Throws<BusinessException>(() =>
            SettlementCalculator.Calculate(
                (decimal)gross,
                (decimal)refund));
    }

    private static void AssertTransitionMatrix<TStatus>(
        IReadOnlyCollection<TStatus> states,
        IReadOnlySet<(TStatus From, TStatus To)> allowed,
        Action<TStatus, TStatus> ensureTransition)
        where TStatus : struct, Enum
    {
        foreach (var current in states)
        {
            foreach (var next in states)
            {
                if (allowed.Contains((current, next)))
                {
                    ensureTransition(current, next);
                    continue;
                }

                var exception = Assert.Throws<BusinessException>(() =>
                    ensureTransition(current, next));
                Assert.False(string.IsNullOrWhiteSpace(exception.Message));
            }
        }
    }

    private static FundingChain CreateValidFundingChain()
    {
        var contractId = Guid.NewGuid();
        var milestone = CreateMilestone(Guid.NewGuid(), contractId);
        milestone.Status = MilestoneStatus.FundedInProgress;
        milestone.FundedAt = UtcNow;

        var account = new EscrowAccount(
            Guid.NewGuid(),
            contractId,
            UtcNow);
        var transactionId = Guid.NewGuid();
        var hold = CreateHold(
            Guid.NewGuid(),
            account.Id,
            contractId,
            milestone.Id,
            transactionId);
        var transaction = new PaymentTransaction(
            transactionId,
            contractId,
            milestone.Id,
            PaymentOperationType.Deposit,
            "Mock",
            Guid.NewGuid().ToString(),
            milestone.Amount,
            UtcNow)
        {
            EscrowHoldId = hold.Id,
            Status = PaymentTransactionStatus.Completed,
            ProcessedAt = UtcNow
        };

        return new FundingChain(
            contractId,
            milestone,
            account,
            hold,
            transaction);
    }

    private static Milestone CreateMilestone(
        Guid milestoneId,
        Guid contractId)
    {
        return new Milestone(
            milestoneId,
            contractId,
            "Draft pleading",
            "Prepare the first pleading.",
            1,
            100m,
            7,
            UtcNow.AddDays(7),
            UtcNow);
    }

    private static EscrowHold CreateHold(
        Guid holdId,
        Guid accountId,
        Guid contractId,
        Guid milestoneId,
        Guid transactionId)
    {
        return new EscrowHold(
            holdId,
            accountId,
            contractId,
            milestoneId,
            100m,
            5m,
            95m,
            transactionId,
            UtcNow,
            UtcNow);
    }

    private static IQueryable<VerifiedMilestoneFunding> Query(
        FundingChain chain,
        FundingVerificationOperation operation)
    {
        return VerifiedMilestoneFundingQuery.Create(
            chain.Milestones.AsQueryable(),
            chain.Accounts.AsQueryable(),
            chain.Holds.AsQueryable(),
            chain.Transactions.AsQueryable(),
            chain.Milestone.Id,
            operation);
    }

    internal enum FundingMismatch
    {
        MissingFundedAt,
        MissingHold,
        MultipleHolds,
        MissingDeposit,
        NonCompletedDeposit,
        MilestoneHoldAmount,
        TransactionAmount,
        TransactionCurrency,
        AccountCurrency,
        WrongHoldStatus,
        WrongHoldReference
    }

    private sealed class FundingChain(
        Guid contractId,
        Milestone milestone,
        EscrowAccount account,
        EscrowHold hold,
        PaymentTransaction transaction)
    {
        internal Guid ContractId { get; } = contractId;
        internal Milestone Milestone { get; } = milestone;
        internal EscrowAccount Account { get; } = account;
        internal EscrowHold Hold { get; } = hold;
        internal PaymentTransaction Transaction { get; } = transaction;
        internal List<Milestone> Milestones { get; } = [milestone];
        internal List<EscrowAccount> Accounts { get; } = [account];
        internal List<EscrowHold> Holds { get; } = [hold];
        internal List<PaymentTransaction> Transactions { get; } = [transaction];
    }
}
