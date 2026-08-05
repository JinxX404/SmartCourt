using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Enums;

namespace SmartCourt.Features.Case.BusinessRules;

internal static class CaseStatusTransitionGuard
{
    private static readonly HashSet<(CaseStatus From, CaseStatus To)> AllowedTransitions =
    [
        (CaseStatus.Draft,          CaseStatus.Submitted),
        (CaseStatus.Submitted,      CaseStatus.Reviewed),
        (CaseStatus.Reviewed,       CaseStatus.Submitted),      // re-review after edit
        (CaseStatus.Reviewed,       CaseStatus.FinalSubmitted),
        (CaseStatus.FinalSubmitted, CaseStatus.Analyzed),
        (CaseStatus.Analyzed,       CaseStatus.Matched),
        (CaseStatus.Matched,        CaseStatus.Closed),
    ];

    internal static void EnsureCanTransition(CaseStatus current, CaseStatus next)
    {
        if (!Enum.IsDefined(current)
            || !Enum.IsDefined(next)
            || !AllowedTransitions.Contains((current, next)))
        {
            throw new BusinessException(
                $"Invalid case status transition from '{current}' to '{next}'.");
        }
    }
}
