using System.Reflection;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Enums;
using Xunit;
using ContractEntity = SmartCourt.Features.Contracts.Entities.Contract;
using ContractAttachmentEntity = SmartCourt.Features.Contracts.Entities.ContractAttachment;
using ContractStateHistoryEntity = SmartCourt.Features.Contracts.Entities.ContractStateHistory;
using DisputeEntity = SmartCourt.Features.Disputes.Entities.Dispute;
using DisputeEvidenceEntity = SmartCourt.Features.Disputes.Entities.DisputeEvidence;
using DisputeResolutionEntity = SmartCourt.Features.Disputes.Entities.DisputeResolution;
using LawyerPenaltyEntity = SmartCourt.Features.Disputes.Entities.LawyerPenalty;
using IdempotencyRecordEntity = SmartCourt.Infrastructure.Persistence.Entities.IdempotencyRecord;
using OutboxMessageEntity = SmartCourt.Infrastructure.Persistence.Entities.OutboxMessage;
using MilestoneEntity = SmartCourt.Features.Milestones.Entities.Milestone;
using MilestoneChangeRequestEntity = SmartCourt.Features.Milestones.Entities.MilestoneChangeRequest;
using MilestoneStateHistoryEntity = SmartCourt.Features.Milestones.Entities.MilestoneStateHistory;
using MilestoneSubmissionEntity = SmartCourt.Features.Milestones.Entities.MilestoneSubmission;
using MilestoneSubmissionAttachmentEntity = SmartCourt.Features.Milestones.Entities.MilestoneSubmissionAttachment;
using EscrowAccountEntity = SmartCourt.Features.Payments.Entities.EscrowAccount;
using EscrowHoldEntity = SmartCourt.Features.Payments.Entities.EscrowHold;
using EscrowLedgerEntryEntity = SmartCourt.Features.Payments.Entities.EscrowLedgerEntry;
using LawyerWalletEntity = SmartCourt.Features.Payments.Entities.LawyerWallet;
using PaymentTransactionEntity = SmartCourt.Features.Payments.Entities.PaymentTransaction;
using WithdrawalRequestEntity = SmartCourt.Features.Payments.Entities.WithdrawalRequest;

namespace SmartCourt.Tests.Features.Contracts.Entities;

public sealed class ContractAndPaymentEntityTests
{
    private static readonly DateTime UtcTimestamp =
        new(2026, 7, 27, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [MemberData(nameof(EntityTypes))]
    public void Entity_DoesNotUseSoftDeleteOrDataAnnotations(Type entityType)
    {
        Assert.False(typeof(BaseEntity).IsAssignableFrom(entityType));
        Assert.False(typeof(AuditableEntity).IsAssignableFrom(entityType));
        Assert.Null(entityType.GetProperty("IsDeleted"));

        var dataAnnotationsNamespace = "System.ComponentModel." + "DataAnnotations";
        var annotatedMembers = entityType
            .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(member => member.GetCustomAttributes()
                .Any(attribute => attribute.GetType().Namespace?.StartsWith(
                    dataAnnotationsNamespace,
                    StringComparison.Ordinal) == true))
            .ToArray();

        Assert.Empty(annotatedMembers);
    }

    [Theory]
    [MemberData(nameof(EntityTypes))]
    public void Entity_ExposesNoPublicMutationSurface(Type entityType)
    {
        Assert.Empty(entityType.GetConstructors(BindingFlags.Public | BindingFlags.Instance));

        var publicSetters = entityType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.SetMethod?.IsPublic == true)
            .ToArray();
        Assert.Empty(publicSetters);

        var publicMethods = entityType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();
        Assert.Empty(publicMethods);
    }

    [Theory]
    [MemberData(nameof(ImmutableEntityTypes))]
    public void AppendOnlyEntity_UsesOnlyPrivateSetters(Type entityType)
    {
        var nonPrivateSetters = entityType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.GetSetMethod(nonPublic: true))
            .Where(setter => setter is not null && !setter.IsPrivate)
            .ToArray();

        Assert.Empty(nonPrivateSetters);
        Assert.Null(entityType.GetProperty("RowVersion"));
    }

    [Theory]
    [MemberData(nameof(RowVersionEntityTypes))]
    public void MutableRoot_HasByteArrayRowVersion(Type entityType)
    {
        var rowVersion = Assert.IsAssignableFrom<PropertyInfo>(
            entityType.GetProperty("RowVersion"));

        Assert.Equal(typeof(byte[]), rowVersion.PropertyType);
        Assert.False(rowVersion.SetMethod?.IsPublic);
    }

    [Theory]
    [MemberData(nameof(RequiredProperties))]
    public void Entity_ContainsRequiredSchemaFields(
        Type entityType,
        string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            Assert.NotNull(entityType.GetProperty(propertyName));
        }
    }

    [Fact]
    public void Contract_StartsAsEgpDraft_WithoutStoredTotal()
    {
        var contract = new ContractEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Commercial representation",
            "Complete terms and conditions.",
            UtcTimestamp);

        Assert.Equal(ContractStatus.Draft, contract.Status);
        Assert.Equal("EGP", contract.Currency);
        Assert.Null(typeof(ContractEntity).GetProperty("TotalAmount"));
        Assert.Equal(UtcTimestamp, contract.CreatedAt);
        Assert.Equal(UtcTimestamp, contract.UpdatedAt);
    }

    [Fact]
    public void Milestone_StartsAsUnfundedDraft_WithoutWritableFundingFlag()
    {
        var milestone = new MilestoneEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Prepare claim",
            "Prepare and file the claim.",
            1,
            10_000m,
            30,
            UtcTimestamp.AddDays(30),
            UtcTimestamp);

        Assert.Equal(MilestoneStatus.Draft, milestone.Status);
        Assert.Equal(MilestoneType.Standard, milestone.Type);
        Assert.Equal(0, milestone.SubmissionVersion);
        Assert.Null(typeof(MilestoneEntity).GetProperty("FundingStatus"));
        Assert.Null(typeof(MilestoneEntity).GetProperty("IsFunded"));
    }

    [Fact]
    public void ExpenseMilestone_RejectsStandardOnlyFields()
    {
        var expense = new MilestoneEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Court filing fee",
            "Reimburse the filing fee.",
            1,
            500m,
            null,
            UtcTimestamp.AddDays(1),
            null,
            UtcTimestamp,
            MilestoneType.Expense);

        Assert.Equal(MilestoneType.Expense, expense.Type);
        Assert.Null(expense.DurationDays);
        Assert.Null(expense.Deliverables);

        Assert.Throws<BusinessException>(() => new MilestoneEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Court filing fee",
            null,
            1,
            500m,
            1,
            null,
            null,
            UtcTimestamp,
            MilestoneType.Expense));
    }

    [Fact]
    public void Submission_RequiresExactFundingIdentityAndImmutableContent()
    {
        var id = Guid.NewGuid();
        var milestoneId = Guid.NewGuid();
        var holdId = Guid.NewGuid();
        var submitterId = Guid.NewGuid();

        var submission = new MilestoneSubmissionEntity(
            id,
            milestoneId,
            holdId,
            submitterId,
            1,
            "Verified funded deliverable.",
            UtcTimestamp);

        Assert.Equal(id, submission.Id);
        Assert.Equal(milestoneId, submission.MilestoneId);
        Assert.Equal(holdId, submission.EscrowHoldId);
        Assert.Equal(submitterId, submission.SubmittedByUserId);
        Assert.Equal(1, submission.Version);

        Assert.Throws<BusinessException>(() => new MilestoneSubmissionEntity(
            id,
            Guid.Empty,
            holdId,
            submitterId,
            1,
            "Notes",
            UtcTimestamp));
        Assert.Throws<BusinessException>(() => new MilestoneSubmissionEntity(
            id,
            milestoneId,
            Guid.Empty,
            submitterId,
            1,
            "Notes",
            UtcTimestamp));
        Assert.Throws<BusinessException>(() => new MilestoneSubmissionEntity(
            id,
            milestoneId,
            holdId,
            Guid.Empty,
            1,
            "Notes",
            UtcTimestamp));
        Assert.Throws<BusinessException>(() => new MilestoneSubmissionEntity(
            id,
            milestoneId,
            holdId,
            submitterId,
            0,
            "Notes",
            UtcTimestamp));
        Assert.Throws<BusinessException>(() => new MilestoneSubmissionEntity(
            id,
            milestoneId,
            holdId,
            submitterId,
            1,
            " ",
            UtcTimestamp));
    }

    [Fact]
    public void EscrowHold_RequiresMatchingGrossFeeAndNet()
    {
        var hold = CreateEscrowHold(10_000m, 500m, 9_500m);

        Assert.Equal(EscrowHoldStatus.Funded, hold.Status);
        Assert.Equal("EGP", new EscrowAccountEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            UtcTimestamp).Currency);

        Assert.Throws<BusinessException>(() =>
            CreateEscrowHold(10_000m, 500m, 9_400m));
    }

    [Fact]
    public void DisputeResolution_IsImmutableAndMustReconcile()
    {
        var resolution = new DisputeResolutionEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DisputeResolutionType.PartialSplit,
            10_000m,
            4_000m,
            5_700m,
            300m,
            "Partial allocation.",
            Guid.NewGuid(),
            UtcTimestamp,
            UtcTimestamp);

        Assert.Equal(10_000m, resolution.GrossHoldAmount);

        Assert.Throws<BusinessException>(() => new DisputeResolutionEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DisputeResolutionType.PartialSplit,
            10_000m,
            4_000m,
            5_500m,
            300m,
            "Does not reconcile.",
            Guid.NewGuid(),
            UtcTimestamp,
            UtcTimestamp));
    }

    [Fact]
    public void DisputeEvidence_RequiresContentOrStoredFile()
    {
        Assert.Throws<BusinessException>(() => new DisputeEvidenceEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            UtcTimestamp));

        var evidence = new DisputeEvidenceEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Written evidence.",
            UtcTimestamp);

        Assert.Equal("Written evidence.", evidence.Content);
    }

    [Fact]
    public void PaymentTransaction_RequiresMilestoneExceptForWithdrawal()
    {
        Assert.Throws<BusinessException>(() => new PaymentTransactionEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            PaymentOperationType.Deposit,
            "Mock",
            "key",
            100m,
            UtcTimestamp));

        var withdrawal = new PaymentTransactionEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            PaymentOperationType.Withdrawal,
            "Mock",
            "key",
            100m,
            UtcTimestamp);

        Assert.Equal(PaymentTransactionStatus.Processing, withdrawal.Status);
    }



    private static EscrowHoldEntity CreateEscrowHold(
        decimal gross,
        decimal fee,
        decimal net)
    {
        return new EscrowHoldEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            gross,
            fee,
            net,
            Guid.NewGuid(),
            UtcTimestamp,
            UtcTimestamp);
    }

    public static TheoryData<Type> EntityTypes => new()
    {
        typeof(ContractEntity),
        typeof(ContractAttachmentEntity),
        typeof(ContractStateHistoryEntity),
        typeof(MilestoneEntity),
        typeof(MilestoneChangeRequestEntity),
        typeof(MilestoneSubmissionEntity),
        typeof(MilestoneSubmissionAttachmentEntity),
        typeof(MilestoneStateHistoryEntity),
        typeof(EscrowAccountEntity),
        typeof(EscrowHoldEntity),
        typeof(EscrowLedgerEntryEntity),
        typeof(PaymentTransactionEntity),
        typeof(LawyerWalletEntity),
        typeof(WithdrawalRequestEntity),
        typeof(DisputeEntity),
        typeof(DisputeResolutionEntity),
        typeof(DisputeEvidenceEntity),
        typeof(LawyerPenaltyEntity),
        typeof(IdempotencyRecordEntity),
        typeof(OutboxMessageEntity)
    };

    public static TheoryData<Type> ImmutableEntityTypes => new()
    {
        typeof(ContractAttachmentEntity),
        typeof(ContractStateHistoryEntity),
        typeof(MilestoneSubmissionEntity),
        typeof(MilestoneSubmissionAttachmentEntity),
        typeof(MilestoneStateHistoryEntity),
        typeof(EscrowLedgerEntryEntity),
        typeof(DisputeResolutionEntity),
        typeof(DisputeEvidenceEntity),
        typeof(LawyerPenaltyEntity)
    };

    public static TheoryData<Type> RowVersionEntityTypes => new()
    {
        typeof(ContractEntity),
        typeof(MilestoneEntity),
        typeof(MilestoneChangeRequestEntity),
        typeof(EscrowAccountEntity),
        typeof(EscrowHoldEntity),
        typeof(PaymentTransactionEntity),
        typeof(LawyerWalletEntity),
        typeof(WithdrawalRequestEntity),
        typeof(DisputeEntity),
        typeof(IdempotencyRecordEntity),
        typeof(OutboxMessageEntity)
    };

    public static TheoryData<Type, string[]> RequiredProperties => new()
    {
        {
            typeof(ContractEntity),
            [
                "Id", "ProposalId", "LegalCaseId", "ClientUserId", "LawyerUserId",
                "Title", "TermsAndConditions", "Currency", "Status",
                "AcceptedByClientAt", "AcceptedByLawyerAt", "ActivatedAt",
                "CompletedAt", "TerminatedAt", "TerminationReason",
                "TerminatedByUserId", "RowVersion", "CreatedAt", "UpdatedAt"
            ]
        },
        {
            typeof(MilestoneEntity),
            [
                "Id", "ContractId", "Title", "Description", "Type", "OrderNumber", "Amount",
                "DurationDays", "DueDate", "Status", "AcceptedByClientAt",
                "AcceptedByLawyerAt", "ReadyForFundingAt", "FundedAt", "SubmittedAt",
                "AutoAcceptEligibleAt", "AutoAcceptJobId", "AcceptedAt",
                "AcceptanceSource", "HoldStartsAt", "HoldExpiresAt", "ReleasedAt",
                "RefundedAt", "RejectionReason", "SubmissionVersion", "RowVersion",
                "CreatedAt", "UpdatedAt"
            ]
        },
        {
            typeof(MilestoneChangeRequestEntity),
            [
                "Id", "MilestoneId", "RequestedByUserId", "ProposedDescription",
                "ProposedDurationDays", "ProposedDueDate", "Reason", "Status",
                "DecidedByUserId", "DecidedAt", "RowVersion", "CreatedAt"
            ]
        },
        {
            typeof(MilestoneSubmissionEntity),
            [
                "Id", "MilestoneId", "EscrowHoldId", "SubmittedByUserId",
                "Version", "Notes", "SubmittedAt"
            ]
        },
        {
            typeof(MilestoneSubmissionAttachmentEntity),
            ["Id", "MilestoneSubmissionId", "StoredFileId", "CreatedAt"]
        },
        {
            typeof(ContractAttachmentEntity),
            ["Id", "ContractId", "StoredFileId", "UploadedByUserId", "CreatedAt"]
        },
        {
            typeof(EscrowAccountEntity),
            [
                "Id", "ContractId", "Currency", "TotalDeposited", "TotalReleased",
                "TotalRefunded", "TotalFees", "Status", "RowVersion", "CreatedAt",
                "UpdatedAt"
            ]
        },
        {
            typeof(EscrowHoldEntity),
            [
                "Id", "EscrowAccountId", "ContractId", "MilestoneId", "GrossAmount",
                "PlatformFeeAmount", "NetAmount", "Status", "FundedAt",
                "HoldStartsAt", "HoldExpiresAt", "FrozenAt", "SettledAt",
                "SettlementType", "ProviderDepositTransactionId",
                "ProviderReleaseTransactionId", "ProviderRefundTransactionId",
                "RowVersion", "CreatedAt", "UpdatedAt"
            ]
        },
        {
            typeof(EscrowLedgerEntryEntity),
            [
                "Id", "EscrowAccountId", "EscrowHoldId", "TransactionType", "Amount",
                "RunningBalance", "Currency", "ReferenceType", "ReferenceId",
                "PaymentTransactionId", "Description", "CreatedByUserId",
                "CorrelationId", "CreatedAt"
            ]
        },
        {
            typeof(PaymentTransactionEntity),
            [
                "Id", "ContractId", "MilestoneId", "EscrowHoldId", "OperationType",
                "ProviderName", "ProviderTransactionId", "IdempotencyKey", "Amount",
                "Currency", "Status", "FailureReason", "ProviderAttemptCount",
                "NextRetryAt", "RequiresManualAction", "ProcessedAt", "RowVersion",
                "CreatedAt", "UpdatedAt"
            ]
        },
        {
            typeof(LawyerWalletEntity),
            [
                "Id", "LawyerUserId", "Currency", "PendingBalance",
                "AvailableBalance", "RowVersion", "CreatedAt", "UpdatedAt"
            ]
        },
        {
            typeof(WithdrawalRequestEntity),
            [
                "Id", "LawyerUserId", "Amount", "Currency", "Status",
                "ProviderTransactionId", "FailureReason", "RequestedAt",
                "ProcessedAt", "IdempotencyKey", "RowVersion"
            ]
        },
        {
            typeof(DisputeEntity),
            [
                "Id", "ContractId", "MilestoneId", "RaisedByUserId",
                "AssignedModeratorUserId", "Category", "Title", "Description",
                "Status", "RequestedOutcome", "ResolutionType", "ResolutionAmount",
                "ResolutionSummary", "ResolvedByUserId", "ResolvedAt", "ClosedAt",
                "RowVersion", "CreatedAt", "UpdatedAt"
            ]
        },
        {
            typeof(DisputeResolutionEntity),
            [
                "Id", "DisputeId", "ResolutionType", "GrossHoldAmount",
                "ClientRefundAmount", "LawyerReleaseAmount", "PlatformFeeAmount",
                "Summary", "ResolvedByUserId", "ResolvedAt", "CreatedAt"
            ]
        },
        {
            typeof(DisputeEvidenceEntity),
            [
                "Id", "DisputeId", "UploadedByUserId", "StoredFileId",
                "Content", "CreatedAt"
            ]
        },
        {
            typeof(LawyerPenaltyEntity),
            [
                "Id", "LawyerUserId", "DisputeId", "PenaltyType", "Reason",
                "StartsAt", "EndsAt", "CreatedByUserId", "CreatedAt"
            ]
        },
        {
            typeof(ContractStateHistoryEntity),
            [
                "Id", "ContractId", "PreviousStatus", "NewStatus", "Trigger",
                "ActorUserId", "Reason", "CorrelationId", "CreatedAt"
            ]
        },
        {
            typeof(MilestoneStateHistoryEntity),
            [
                "Id", "MilestoneId", "PreviousStatus", "NewStatus", "Trigger",
                "ActorUserId", "Reason", "CorrelationId", "CreatedAt"
            ]
        },
        {
            typeof(IdempotencyRecordEntity),
            [
                "Id", "UserId", "Key", "Operation", "ResourceType", "ResourceId",
                "RequestHash", "Status", "ResponseStatusCode", "ResponseBody",
                "ResultReferenceId", "ExpiresAt", "CompletedAt", "RowVersion",
                "CreatedAt"
            ]
        },
        {
            typeof(OutboxMessageEntity),
            [
                "Id", "EventType", "EventVersion", "Payload", "AggregateType",
                "AggregateId", "CorrelationId", "Status", "Attempts", "LastError",
                "AvailableAt", "ProcessedAt", "RowVersion", "CreatedAt"
            ]
        }
    };
}
