using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Case.BusinessRules;
using Xunit;

namespace SmartCourt.Tests.Features.Case.Domain;

public sealed class CaseStatusTransitionGuardTests
{
    [Fact]
    public void CaseStatusTransitionGuard_AllowsOnlyDocumentedTransitions()
    {
        HashSet<(CaseStatus From, CaseStatus To)> allowed =
        [
            (CaseStatus.Draft,          CaseStatus.Submitted),
            (CaseStatus.Submitted,      CaseStatus.Reviewed),
            (CaseStatus.Reviewed,       CaseStatus.Submitted),
            (CaseStatus.Reviewed,       CaseStatus.FinalSubmitted),
            (CaseStatus.FinalSubmitted, CaseStatus.Analyzed),
            (CaseStatus.Analyzed,       CaseStatus.Matched),
            (CaseStatus.Matched,        CaseStatus.Closed),
        ];

        AssertTransitionMatrix(
            Enum.GetValues<CaseStatus>(),
            allowed,
            CaseStatusTransitionGuard.EnsureCanTransition);
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
}
