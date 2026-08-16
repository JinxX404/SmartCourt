using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Users.Lawyers.Dashboard;
using SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;
using Xunit;

namespace SmartCourt.Tests.Features.Users.Lawyers.Dashboard;

public sealed class LawyerDashboardControllerTests
{
    private sealed class StubDashboardService : ILawyerDashboardService
    {
        public LawyerDashboardStatsDto StatsToReturn { get; set; } = new(
            ActiveContractsCount: 3,
            PendingProposalsCount: 2,
            RecommendedCasesCount: 5,
            UpcomingConsultationsCount: 1,
            PendingConsultationRequestsCount: 0,
            AvailableBalance: 15000m,
            PendingBalance: 4000m,
            LifetimeEarnings: 35000m,
            Currency: "EGP",
            AverageRating: 4.9m,
            TotalReviewsCount: 25,
            UnreadNotificationsCount: 3,
            ActiveDisputesCount: 0
        );

        public PagedResult<LawyerActivityItemDto> ActivityToReturn { get; set; } = new(
            [
                new LawyerActivityItemDto(
                    Guid.NewGuid(),
                    LawyerActivityType.ContractStateChanged,
                    "عقد جديد",
                    "تم تفعيل العقد",
                    Guid.NewGuid(),
                    "Contract",
                    DateTimeOffset.UtcNow)
            ],
            1,
            15,
            1,
            false);

        public LawyerEarningsSummaryDto EarningsToReturn { get; set; } = new(
            TotalEarnedInPeriod: 12000m,
            ContractEarningsInPeriod: 10000m,
            ConsultationEarningsInPeriod: 2000m,
            PlatformFeesPaidInPeriod: 200m,
            CurrentAvailableBalance: 8000m,
            CurrentPendingBalance: 1500m,
            Currency: "EGP",
            PeriodBreakdown: [],
            RecentWithdrawals: []
        );

        public IReadOnlyList<UpcomingDeadlineItemDto> DeadlinesToReturn { get; set; } =
        [
            new UpcomingDeadlineItemDto(
                ContractId: Guid.NewGuid(),
                ContractTitle: "عقد استشارة",
                MilestoneId: Guid.NewGuid(),
                MilestoneTitle: "مرحلة أولى",
                MilestoneOrder: 1,
                Amount: 5000m,
                Currency: "EGP",
                MilestoneStatus: "FundedInProgress",
                DueDateUtc: DateTimeOffset.UtcNow.AddDays(3),
                DaysRemaining: 3,
                Urgency: DeadlineUrgency.Approaching,
                ClientId: Guid.NewGuid(),
                ClientName: "عميل تجريبي"
            )
        ];

        public LawyerCalendarScheduleDto CalendarToReturn { get; set; } = new(
            FromUtc: DateTimeOffset.UtcNow,
            ToUtc: DateTimeOffset.UtcNow.AddDays(30),
            Events:
            [
                new LawyerCalendarEventDto(
                    Id: Guid.NewGuid(),
                    EventType: CalendarEventType.Consultation,
                    Title: "استشارة قانونية",
                    Description: "تفاصيل الاستشارة",
                    StartUtc: DateTimeOffset.UtcNow.AddDays(1),
                    EndUtc: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
                    Status: "Confirmed",
                    ReferenceId: Guid.NewGuid(),
                    ReferenceType: "Consultation",
                    ClientId: Guid.NewGuid(),
                    ClientName: "عميل",
                    MeetingUrlOrLocation: "https://meet.test.com"
                )
            ]
        );

        public Task<LawyerDashboardStatsDto> GetStatsAsync(
            Guid lawyerUserId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StatsToReturn);
        }

        public Task<PagedResult<LawyerActivityItemDto>> GetActivityAsync(
            Guid lawyerUserId,
            LawyerActivityQuery query,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ActivityToReturn);
        }

        public Task<LawyerEarningsSummaryDto> GetEarningsAsync(
            Guid lawyerUserId,
            LawyerEarningsQuery query,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EarningsToReturn);
        }

        public Task<IReadOnlyList<UpcomingDeadlineItemDto>> GetUpcomingDeadlinesAsync(
            Guid lawyerUserId,
            LawyerDeadlinesQuery query,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(DeadlinesToReturn);
        }

        public Task<LawyerCalendarScheduleDto> GetCalendarScheduleAsync(
            Guid lawyerUserId,
            LawyerCalendarQuery query,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CalendarToReturn);
        }
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsOkApiResponseWithStats()
    {
        var stubService = new StubDashboardService();
        var controller = new LawyerDashboardController(stubService);
        var lawyerUserId = Guid.NewGuid();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, lawyerUserId.ToString()),
            new Claim(ClaimTypes.Role, "Lawyer")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var actionResult = await controller.GetStatsAsync(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var response = Assert.IsType<ApiResponse<LawyerDashboardStatsDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal(stubService.StatsToReturn, response.Data);
    }

    [Fact]
    public async Task GetActivityAsync_ReturnsOkApiResponseWithPagedActivity()
    {
        var stubService = new StubDashboardService();
        var controller = new LawyerDashboardController(stubService);
        var lawyerUserId = Guid.NewGuid();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, lawyerUserId.ToString()),
            new Claim(ClaimTypes.Role, "Lawyer")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var query = new LawyerActivityQuery(1, 15);
        var actionResult = await controller.GetActivityAsync(query, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var response = Assert.IsType<ApiResponse<PagedResult<LawyerActivityItemDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal(stubService.ActivityToReturn, response.Data);
    }

    [Fact]
    public async Task GetEarningsAsync_ReturnsOkApiResponseWithEarnings()
    {
        var stubService = new StubDashboardService();
        var controller = new LawyerDashboardController(stubService);
        var lawyerUserId = Guid.NewGuid();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, lawyerUserId.ToString()),
            new Claim(ClaimTypes.Role, "Lawyer")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var query = new LawyerEarningsQuery("6months", "monthly");
        var actionResult = await controller.GetEarningsAsync(query, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var response = Assert.IsType<ApiResponse<LawyerEarningsSummaryDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal(stubService.EarningsToReturn, response.Data);
    }

    [Fact]
    public async Task GetUpcomingDeadlinesAsync_ReturnsOkApiResponseWithDeadlines()
    {
        var stubService = new StubDashboardService();
        var controller = new LawyerDashboardController(stubService);
        var lawyerUserId = Guid.NewGuid();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, lawyerUserId.ToString()),
            new Claim(ClaimTypes.Role, "Lawyer")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var query = new LawyerDeadlinesQuery(30);
        var actionResult = await controller.GetUpcomingDeadlinesAsync(query, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var response = Assert.IsType<ApiResponse<IReadOnlyList<UpcomingDeadlineItemDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal(stubService.DeadlinesToReturn, response.Data);
    }

    [Fact]
    public async Task GetCalendarScheduleAsync_ReturnsOkApiResponseWithSchedule()
    {
        var stubService = new StubDashboardService();
        var controller = new LawyerDashboardController(stubService);
        var lawyerUserId = Guid.NewGuid();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, lawyerUserId.ToString()),
            new Claim(ClaimTypes.Role, "Lawyer")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var query = new LawyerCalendarQuery();
        var actionResult = await controller.GetCalendarScheduleAsync(query, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var response = Assert.IsType<ApiResponse<LawyerCalendarScheduleDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal(stubService.CalendarToReturn, response.Data);
    }
}
