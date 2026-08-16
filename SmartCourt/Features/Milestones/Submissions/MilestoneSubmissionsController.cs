using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones;

[ApiController]
[Route("api/milestones/{milestoneId:guid}/submissions")]
[Authorize(Roles = "Client,Lawyer,Moderator,SuperAdministrator")]
public sealed class MilestoneSubmissionsController(
    IMilestoneSubmissionQueryService service) : ControllerBase
{
    [HttpGet]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MilestoneSubmissionDto>>>>
        ListAsync(
            Guid milestoneId,
            CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<MilestoneSubmissionDto>>.Ok(
            await service.ListAsync(milestoneId, cancellationToken)));

    [HttpGet("{submissionId:guid}/files/{storedFileId:guid}")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    public async Task<ActionResult<ApiResponse<MilestoneSubmissionFileAccessDto>>>
        GetFileAccessAsync(
            Guid milestoneId,
            Guid submissionId,
            Guid storedFileId,
            CancellationToken cancellationToken)
        => Ok(ApiResponse<MilestoneSubmissionFileAccessDto>.Ok(
            await service.GetFileAccessAsync(
                milestoneId,
                submissionId,
                storedFileId,
                cancellationToken)));
}
