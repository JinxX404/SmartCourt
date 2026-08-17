using SmartCourt.Common.Enums;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Persistence.Enums;
using Xunit;

namespace SmartCourt.Tests.Common.Enums;

public sealed class ContractAndPaymentEnumTests
{
    [Theory]
    [MemberData(nameof(ExactV1Enums))]
    public void ExactV1Enum_HasDocumentedNamesAndOrdinals(
        Type enumType,
        string[] expected)
    {
        AssertEnumDefinition(enumType, expected);
    }

    [Theory]
    [MemberData(nameof(SupportingPersistenceEnums))]
    public void SupportingEnum_HasStableNamesAndOrdinals(
        Type enumType,
        string[] expected)
    {
        AssertEnumDefinition(enumType, expected);
    }

    [Theory]
    [MemberData(nameof(AllPersistedEnums))]
    public void PersistedEnum_IsContiguousFromZero_ForEfCheckConstraints(
        Type enumType)
    {
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(enumType));

        var values = Enum.GetValues(enumType)
            .Cast<int>()
            .Order()
            .ToArray();

        Assert.Equal(Enumerable.Range(0, values.Length), values);
        Assert.False(Enum.IsDefined(enumType, -1));
        Assert.False(Enum.IsDefined(enumType, values.Length));
    }

    [Theory]
    [MemberData(nameof(EnumOwnership))]
    public void PersistedEnum_LivesWithItsOwningSlice(
        Type enumType,
        string expectedNamespace)
    {
        Assert.Equal(expectedNamespace, enumType.Namespace);
    }

    public static TheoryData<Type, string[]> ExactV1Enums => new()
    {
        {
            typeof(ContractStatus),
            ["Draft=0", "Active=1", "SuspendedByDispute=2", "Completed=3", "Terminated=4", "CompletedOnHold=5"]
        },
        {
            typeof(MilestoneStatus),
            [
                "Draft=0",
                "AwaitingFunding=1",
                "FundingProcessing=2",
                "FundedInProgress=3",
                "Submitted=4",
                "AcceptedHold=5",
                "Disputed=6",
                "Released=7",
                "Refunded=8",
                "Cancelled=9",
                "ReleasePending=10"
            ]
        },
        {
            typeof(MilestoneType),
            ["Standard=0", "Expense=1"]
        },
        {
            typeof(EscrowHoldStatus),
            ["Funded=0", "Frozen=1", "Released=2", "Refunded=3"]
        },
        {
            typeof(MilestoneFundingStatus),
            ["Unfunded=0", "Processing=1", "Funded=2", "Settled=3"]
        },
        {
            typeof(MilestoneAcceptanceSource),
            ["Manual=0", "Automatic=1"]
        },
        {
            typeof(DisputeStatus),
            ["Open=0", "Assigned=1", "UnderReview=2", "Resolved=3", "Closed=4", "Cancelled=5"]
        },
        {
            typeof(DisputeResolutionType),
            ["FullRefund=0", "FullRelease=1", "PartialSplit=2"]
        },
        {
            typeof(ChangeRequestStatus),
            ["Pending=0", "Approved=1", "Rejected=2", "Cancelled=3"]
        },
        {
            typeof(PenaltyType),
            [
                "Warning=0",
                "Suspension12Months=1",
                "Suspension24Months=2",
                "PermanentTermination=3"
            ]
        }
    };

    public static TheoryData<Type, string[]> SupportingPersistenceEnums => new()
    {
        {
            typeof(PaymentOperationType),
            ["Deposit=0", "Release=1", "Refund=2", "Withdrawal=3"]
        },
        {
            typeof(PaymentTransactionStatus),
            ["Processing=0", "Completed=1", "Failed=2"]
        },
        {
            typeof(LedgerTransactionType),
            ["Deposit=0", "Release=1", "Refund=2", "PlatformFee=3", "Adjustment=4"]
        },
        {
            typeof(SettlementType),
            ["Release=0", "Refund=1", "PartialSplit=2"]
        },
        {
            typeof(EscrowAccountStatus),
            ["Active=0", "Closed=1"]
        },
        {
            typeof(WithdrawalStatus),
            ["Processing=0", "Completed=1", "Failed=2"]
        },
        {
            typeof(DisputeCategory),
            [
                "NonDelivery=0",
                "DeliverableQuality=1",
                "Misrepresentation=2",
                "Payment=3",
                "ContractTerms=4",
                "Other=5"
            ]
        },
        {
            typeof(DisputeRequestedOutcome),
            ["Refund=0", "Release=1", "Review=2"]
        },
        {
            typeof(IdempotencyStatus),
            ["Processing=0", "Completed=1", "Failed=2"]
        },
        {
            typeof(OutboxStatus),
            ["Pending=0", "Processing=1", "Processed=2", "Failed=3"]
        }
    };

    public static TheoryData<Type> AllPersistedEnums
    {
        get
        {
            var data = new TheoryData<Type>();

            foreach (var row in ExactV1Enums)
            {
                data.Add((Type)row[0]);
            }

            foreach (var row in SupportingPersistenceEnums)
            {
                data.Add((Type)row[0]);
            }

            return data;
        }
    }

    public static TheoryData<Type, string> EnumOwnership => new()
    {
        { typeof(ContractStatus), "SmartCourt.Features.Contracts.Enums" },
        { typeof(MilestoneStatus), "SmartCourt.Features.Milestones.Enums" },
        { typeof(MilestoneType), "SmartCourt.Features.Milestones.Enums" },
        { typeof(MilestoneFundingStatus), "SmartCourt.Features.Milestones.Enums" },
        { typeof(MilestoneAcceptanceSource), "SmartCourt.Features.Milestones.Enums" },
        { typeof(ChangeRequestStatus), "SmartCourt.Features.Milestones.Enums" },
        { typeof(EscrowHoldStatus), "SmartCourt.Features.Payments.Enums" },
        { typeof(PaymentOperationType), "SmartCourt.Features.Payments.Enums" },
        { typeof(PaymentTransactionStatus), "SmartCourt.Features.Payments.Enums" },
        { typeof(LedgerTransactionType), "SmartCourt.Features.Payments.Enums" },
        { typeof(SettlementType), "SmartCourt.Features.Payments.Enums" },
        { typeof(EscrowAccountStatus), "SmartCourt.Features.Payments.Enums" },
        { typeof(WithdrawalStatus), "SmartCourt.Features.Payments.Enums" },
        { typeof(DisputeStatus), "SmartCourt.Features.Disputes.Enums" },
        { typeof(DisputeResolutionType), "SmartCourt.Features.Disputes.Enums" },
        { typeof(PenaltyType), "SmartCourt.Features.Disputes.Enums" },
        { typeof(DisputeCategory), "SmartCourt.Features.Disputes.Enums" },
        { typeof(DisputeRequestedOutcome), "SmartCourt.Features.Disputes.Enums" },
        {
            typeof(IdempotencyStatus),
            "SmartCourt.Infrastructure.Persistence.Enums"
        },
        {
            typeof(OutboxStatus),
            "SmartCourt.Infrastructure.Persistence.Enums"
        }
    };

    private static void AssertEnumDefinition(Type enumType, string[] expected)
    {
        var actual = Enum.GetNames(enumType)
            .Select(name => $"{name}={Convert.ToInt32(Enum.Parse(enumType, name))}")
            .ToArray();

        Assert.Equal(expected, actual);
    }
}
