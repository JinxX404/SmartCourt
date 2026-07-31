using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones;

public sealed class MilestoneChangeRequestService(
    IMilestoneService milestoneService) : IMilestoneChangeRequestService
{
    public async Task<MilestoneActionResultDto> CreateChangeRequestAsync(
        Guid milestoneId,
        CreateMilestoneChangeRequest request,
        string ifMatch,
        CancellationToken cancellationToken)
    {
        return await milestoneService.CreateChangeRequestAsync(
            milestoneId,
            request,
            ifMatch,
            cancellationToken);
    }

    public async Task<MilestoneActionResultDto> ApproveChangeRequestAsync(
        Guid changeRequestId,
        string ifMatch,
        CancellationToken cancellationToken)
    {
        return await milestoneService.ApproveChangeRequestAsync(
            changeRequestId,
            ifMatch,
            cancellationToken);
    }

    public async Task<MilestoneActionResultDto> RejectChangeRequestAsync(
        Guid changeRequestId,
        RejectChangeRequest request,
        string ifMatch,
        CancellationToken cancellationToken)
    {
        return await milestoneService.RejectChangeRequestAsync(
            changeRequestId,
            request,
            ifMatch,
            cancellationToken);
    }

    public async Task<MilestoneActionResultDto> CancelChangeRequestAsync(
        Guid changeRequestId,
        string ifMatch,
        CancellationToken cancellationToken)
    {
        return await milestoneService.CancelChangeRequestAsync(
            changeRequestId,
            ifMatch,
            cancellationToken);
    }
}
