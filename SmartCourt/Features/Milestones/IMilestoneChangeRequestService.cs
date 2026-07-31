using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones;

public interface IMilestoneChangeRequestService
{
    Task<MilestoneActionResultDto> CreateChangeRequestAsync(
        Guid milestoneId,
        CreateMilestoneChangeRequest request,
        string ifMatch,
        CancellationToken cancellationToken);

    Task<MilestoneActionResultDto> ApproveChangeRequestAsync(
        Guid changeRequestId,
        string ifMatch,
        CancellationToken cancellationToken);

    Task<MilestoneActionResultDto> RejectChangeRequestAsync(
        Guid changeRequestId,
        RejectChangeRequest request,
        string ifMatch,
        CancellationToken cancellationToken);

    Task<MilestoneActionResultDto> CancelChangeRequestAsync(
        Guid changeRequestId,
        string ifMatch,
        CancellationToken cancellationToken);
}
