using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones;

public interface IMilestoneService
{
    Task<MilestoneDto> AddAsync(
        Guid contractId,
        AddMilestoneRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<MilestoneDto>> ListAsync(
        Guid contractId,
        CancellationToken cancellationToken);

    Task<MilestoneDto> UpdateDraftAsync(
        Guid contractId,
        Guid milestoneId,
        UpdateMilestoneRequest request,
        string ifMatch,
        CancellationToken cancellationToken);

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
