using SmartCourt.Common.Models;
using SmartCourt.Features.Ratings.DTOs;

namespace SmartCourt.Features.Ratings;

public interface IRatingService
{
    Task<ContractRatingDto> SubmitAsync(
        Guid contractId,
        SubmitRatingRequest request,
        CancellationToken cancellationToken);

    Task<ContractRatingDto> UpdateAsync(
        Guid contractId,
        UpdateRatingRequest request,
        CancellationToken cancellationToken);

    Task<ContractRatingSummaryDto> GetByContractAsync(
        Guid contractId,
        CancellationToken cancellationToken);


    Task<PagedResult<ContractRatingDto>> GetByLawyerAsync(
        Guid lawyerUserId,
        LawyerRatingsQuery query,
        CancellationToken cancellationToken);
}
