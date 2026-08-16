using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones;

public interface IMilestoneSubmissionQueryService
{
    Task<IReadOnlyList<MilestoneSubmissionDto>> ListAsync(
        Guid milestoneId,
        CancellationToken cancellationToken);

    Task<MilestoneSubmissionFileAccessDto> GetFileAccessAsync(
        Guid milestoneId,
        Guid submissionId,
        Guid storedFileId,
        CancellationToken cancellationToken);
}
