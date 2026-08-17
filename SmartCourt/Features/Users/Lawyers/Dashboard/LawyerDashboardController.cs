using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Extensions;
using SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;

namespace SmartCourt.Features.Users.Lawyers.Dashboard;

[ApiController]
[Route("api/lawyers/dashboard")]
[Authorize(Roles = "Lawyer,Admin,SuperAdministrator")]
public sealed class LawyerDashboardController(ILawyerDashboardService dashboardService) : ControllerBase
{
    private readonly ILawyerDashboardService _dashboardService = dashboardService;

    [HttpGet("stats")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [ProducesResponseType(
        typeof(ApiResponse<LawyerDashboardStatsDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LawyerDashboardStatsDto>>> GetStatsAsync(
        CancellationToken cancellationToken)
    {
        var lawyerUserId = User.GetUserIdAsGuid();
        var result = await _dashboardService.GetStatsAsync(lawyerUserId, cancellationToken);
        return Ok(ApiResponse<LawyerDashboardStatsDto>.Ok(result));
    }

    [HttpGet("activity")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<LawyerActivityItemDto>>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<LawyerActivityItemDto>>>> GetActivityAsync(
        [FromQuery] LawyerActivityQuery query,
        CancellationToken cancellationToken)
    {
        var lawyerUserId = User.GetUserIdAsGuid();
        var result = await _dashboardService.GetActivityAsync(lawyerUserId, query, cancellationToken);
        return Ok(ApiResponse<PagedResult<LawyerActivityItemDto>>.Ok(result));
    }

    [HttpGet("earnings")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [ProducesResponseType(
        typeof(ApiResponse<LawyerEarningsSummaryDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LawyerEarningsSummaryDto>>> GetEarningsAsync(
        [FromQuery] LawyerEarningsQuery query,
        CancellationToken cancellationToken)
    {
        var lawyerUserId = User.GetUserIdAsGuid();
        var result = await _dashboardService.GetEarningsAsync(lawyerUserId, query, cancellationToken);
        return Ok(ApiResponse<LawyerEarningsSummaryDto>.Ok(result));
    }

    [HttpGet("upcoming-deadlines")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyList<UpcomingDeadlineItemDto>>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UpcomingDeadlineItemDto>>>> GetUpcomingDeadlinesAsync(
        [FromQuery] LawyerDeadlinesQuery query,
        CancellationToken cancellationToken)
    {
        var lawyerUserId = User.GetUserIdAsGuid();
        var result = await _dashboardService.GetUpcomingDeadlinesAsync(lawyerUserId, query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UpcomingDeadlineItemDto>>.Ok(result));
    }

    [HttpGet("calendar")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [ProducesResponseType(
        typeof(ApiResponse<LawyerCalendarScheduleDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LawyerCalendarScheduleDto>>> GetCalendarScheduleAsync(
        [FromQuery] LawyerCalendarQuery query,
        CancellationToken cancellationToken)
    {
        var lawyerUserId = User.GetUserIdAsGuid();
        var result = await _dashboardService.GetCalendarScheduleAsync(lawyerUserId, query, cancellationToken);
        return Ok(ApiResponse<LawyerCalendarScheduleDto>.Ok(result));
    }
}
