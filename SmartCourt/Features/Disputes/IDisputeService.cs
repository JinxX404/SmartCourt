using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Disputes.DTOs;

namespace SmartCourt.Features.Disputes;

public interface IDisputeService
{
    Task<DisputeDto> CreateAsync(
        CreateDisputeRequest request,
        CancellationToken cancellationToken);

    Task<PagedResult<DisputeDto>> ListAsync(
        DisputeListQuery query,
        CancellationToken cancellationToken);

    Task<DisputeDto> GetAsync(
        Guid disputeId,
        CancellationToken cancellationToken);

    Task<DisputeActionResultDto> AddEvidenceAsync(
        Guid disputeId,
        AddDisputeEvidenceRequest request,
        CancellationToken cancellationToken);

    Task<DisputeDto> AssignAsync(
        Guid disputeId,
        AssignDisputeRequest request,
        CancellationToken cancellationToken);

    Task<DisputeDto> StartReviewAsync(
        Guid disputeId,
        CancellationToken cancellationToken);

    Task<DisputeDto> ResolveAsync(
        Guid disputeId,
        ResolveDisputeRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken);

    Task<DisputeActionResultDto> CloseAsync(
        Guid disputeId,
        CancellationToken cancellationToken);
}
