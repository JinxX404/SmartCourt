using SmartCourt.Common.Models;
using SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;

namespace SmartCourt.Features.Users.Lawyers.Dashboard;

public interface ILawyerDashboardService
{
    Task<LawyerDashboardStatsDto> GetStatsAsync(Guid lawyerUserId, CancellationToken cancellationToken = default);

    Task<PagedResult<LawyerActivityItemDto>> GetActivityAsync(
        Guid lawyerUserId,
        LawyerActivityQuery query,
        CancellationToken cancellationToken = default);

    Task<LawyerEarningsSummaryDto> GetEarningsAsync(
        Guid lawyerUserId,
        LawyerEarningsQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UpcomingDeadlineItemDto>> GetUpcomingDeadlinesAsync(
        Guid lawyerUserId,
        LawyerDeadlinesQuery query,
        CancellationToken cancellationToken = default);

    Task<LawyerCalendarScheduleDto> GetCalendarScheduleAsync(
        Guid lawyerUserId,
        LawyerCalendarQuery query,
        CancellationToken cancellationToken = default);
}
