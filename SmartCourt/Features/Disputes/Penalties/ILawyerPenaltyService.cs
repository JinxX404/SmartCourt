using SmartCourt.Common.Models;
using SmartCourt.Features.Disputes.DTOs;

namespace SmartCourt.Features.Disputes.Penalties;

public interface ILawyerPenaltyService
{
    Task<PagedResult<LawyerPenaltyDto>> ListAsync(
        LawyerPenaltyFilterQuery query,
        CancellationToken cancellationToken);

    Task<PagedResult<LawyerPenaltyDto>> GetMyPenaltiesAsync(
        LawyerPenaltyFilterQuery query,
        CancellationToken cancellationToken);

    Task<LawyerPenaltyDto> RevokeAsync(
        Guid penaltyId,
        RevokeLawyerPenaltyRequest request,
        CancellationToken cancellationToken);
}
