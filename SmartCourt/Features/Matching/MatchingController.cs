using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Matching.DTOs;
using SmartCourt.Interfaces;

namespace SmartCourt.Features.Matching;

[ApiController]
[Route("api/cases")]
[Authorize]
public class MatchingController(
    IMatchingService matchingService,
    ICurrentUserService currentUserService) : ControllerBase
{
    private readonly IMatchingService _matchingService = matchingService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    [HttpGet("{id:guid}/recommendations")]
    public async Task<ActionResult<PagedResponse<FinalizeResultDto>>> GetRecommendations(
        [FromRoute] Guid id,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("المستخدم غير مصرح له.");

        var result = await _matchingService.GetRecommendationsAsync(id, currentUserId, request, cancellationToken);
        return Ok(result);
    }
}
