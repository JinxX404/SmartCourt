using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones;

public interface IMilestoneService
{
    Task<MilestoneActionResultDto> ApproveAsync(
        Guid milestoneId,
        string ifMatch,
        CancellationToken cancellationToken);

    Task<MilestoneActionResultDto> MarkReadyForFundingAsync(
        Guid milestoneId,
        string ifMatch,
        CancellationToken cancellationToken);

    Task<MilestoneDto> SubmitAsync(
        Guid milestoneId,
        SubmitMilestoneRequest request,
        CancellationToken cancellationToken);

    Task<MilestoneDto> AcceptAsync(
        Guid milestoneId,
        CancellationToken cancellationToken);

    Task<MilestoneDto> RequestChangesAsync(
        Guid milestoneId,
        RequestMilestoneChangesRequest request,
        CancellationToken cancellationToken);
}
