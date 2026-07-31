using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones;

public sealed class MilestoneDraftService(
    IMilestoneService milestoneService) : IMilestoneDraftService
{
    public async Task<MilestoneDto> AddAsync(
        Guid contractId,
        AddMilestoneRequest request,
        CancellationToken cancellationToken)
    {
        return await milestoneService.AddAsync(
            contractId,
            request,
            cancellationToken);
    }

    public async Task<IReadOnlyList<MilestoneDto>> ListAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        return await milestoneService.ListAsync(
            contractId,
            cancellationToken);
    }

    public async Task<MilestoneDto> UpdateDraftAsync(
        Guid contractId,
        Guid milestoneId,
        UpdateMilestoneRequest request,
        string ifMatch,
        CancellationToken cancellationToken)
    {
        return await milestoneService.UpdateDraftAsync(
            contractId,
            milestoneId,
            request,
            ifMatch,
            cancellationToken);
    }
}
