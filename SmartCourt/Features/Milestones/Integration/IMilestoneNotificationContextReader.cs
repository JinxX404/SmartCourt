using SmartCourt.Features.Milestones.Enums;

namespace SmartCourt.Features.Milestones.Integration;

public interface IMilestoneNotificationContextReader
{
    Task<MilestoneNotificationContext> GetMilestoneAsync(
        Guid milestoneId,
        CancellationToken cancellationToken);

    Task<MilestoneChangeRequestNotificationContext> GetChangeRequestAsync(
        Guid changeRequestId,
        CancellationToken cancellationToken);
}

public sealed record MilestoneNotificationContext(
    Guid MilestoneId,
    Guid ContractId,
    Guid ProposalId,
    Guid LegalCaseId,
    Guid ClientUserId,
    Guid LawyerUserId,
    MilestoneType Type = MilestoneType.Standard);

public sealed record MilestoneChangeRequestNotificationContext(
    Guid ChangeRequestId,
    Guid MilestoneId,
    Guid ContractId,
    Guid ProposalId,
    Guid LegalCaseId,
    Guid ClientUserId,
    Guid LawyerUserId,
    Guid RequestedByUserId);
