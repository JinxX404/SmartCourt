using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;
using SmartCourt.Persistence;
using SmartCourt.Tests.Common;
using Xunit;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Tests.Features.Users.Lawyers.Dashboard;

public class LawyerDashboardApiE2ETests : IClassFixture<SmartCourtWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SmartCourtWebApplicationFactory _factory;

    public LawyerDashboardApiE2ETests(SmartCourtWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetStats_AuthenticatedLawyer_Returns200WithStats()
    {
        var lawyerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var nowUtc = DateTimeOffset.UtcNow;

        await _factory.SeedUserAsync(lawyerId, $"lawyer_{lawyerId:N}@test.com", "Lawyer", "Test Lawyer");
        await _factory.SeedUserAsync(clientId, $"client_{clientId:N}@test.com", "Client", "Test Client");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var profile = new LawyerProfile
            {
                UserId = lawyerId,
                AverageRating = 4.75m,
                TotalRatingCount = 8,
                TotalRatingSum = 38
            };
            db.LawyerProfiles.Add(profile);

            var wallet = new LawyerWallet(Guid.NewGuid(), lawyerId, nowUtc.AddMonths(-1))
            {
                AvailableBalance = 12000m,
                PendingBalance = 3000m,
                Currency = "EGP"
            };
            db.LawyerWallets.Add(wallet);

            var caseEntity = new CaseEntity
            {
                Id = caseId,
                ClientId = clientId,
                Title = "قضية تجارية",
                Description = "نزاع تجاري",
                City = "القاهرة",
                SubmittedAt = nowUtc.AddDays(-10),
                Status = CaseStatus.Matched
            };
            db.Cases.Add(caseEntity);

            var proposal = new Proposal(
                proposalId,
                caseId,
                clientId,
                lawyerId,
                "عرض جديد",
                nowUtc.AddDays(-1))
            {
                Status = ProposalStatus.Pending
            };
            db.Proposals.Add(proposal);

            var activeContract = new Contract(
                contractId,
                proposalId,
                caseId,
                clientId,
                lawyerId,
                "عقد استشارة",
                "شروط العقد",
                nowUtc.AddDays(-5))
            {
                Status = ContractStatus.Active
            };
            db.Contracts.Add(activeContract);

            await db.SaveChangesAsync();
        }

        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var response = await lawyerClient.GetAsync("/api/lawyers/dashboard/stats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LawyerDashboardStatsDto>>(JsonOptions);
        Assert.NotNull(result?.Data);
        Assert.Equal(1, result.Data.ActiveContractsCount);
        Assert.Equal(1, result.Data.PendingProposalsCount);
        Assert.Equal(12000m, result.Data.AvailableBalance);
        Assert.Equal(3000m, result.Data.PendingBalance);
        Assert.Equal(4.75m, result.Data.AverageRating);
        Assert.Equal(8, result.Data.TotalReviewsCount);
    }

    [Fact]
    public async Task GetStats_ClientRole_Returns403Forbidden()
    {
        var clientId = Guid.NewGuid();
        await _factory.SeedUserAsync(clientId, $"client_{clientId:N}@test.com", "Client", "Test Client");

        var clientHttp = _factory.CreateAuthenticatedClient(clientId, "Client");

        var response = await clientHttp.GetAsync("/api/lawyers/dashboard/stats");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetActivity_AuthenticatedLawyer_Returns200WithPagedActivity()
    {
        var lawyerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var nowUtc = DateTimeOffset.UtcNow;

        await _factory.SeedUserAsync(lawyerId, $"lawyer_{lawyerId:N}@test.com", "Lawyer", "Test Lawyer Activity");
        await _factory.SeedUserAsync(clientId, $"client_{clientId:N}@test.com", "Client", "Test Client Activity");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var caseEntity = new CaseEntity
            {
                Id = caseId,
                ClientId = clientId,
                Title = "قضية تجارية للاختبار",
                Description = "نزاع تجاري",
                City = "القاهرة",
                SubmittedAt = nowUtc.AddDays(-10),
                Status = CaseStatus.Matched
            };
            db.Cases.Add(caseEntity);

            var proposal = new Proposal(
                proposalId,
                caseId,
                clientId,
                lawyerId,
                "عرض جديد للنشاط",
                nowUtc.AddDays(-2))
            {
                Status = ProposalStatus.Pending
            };
            db.Proposals.Add(proposal);

            await db.SaveChangesAsync();
        }

        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var response = await lawyerClient.GetAsync("/api/lawyers/dashboard/activity?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<LawyerActivityItemDto>>>(JsonOptions);
        Assert.NotNull(result?.Data);
        Assert.Single(result.Data.Items);
        Assert.Equal(LawyerActivityType.ProposalStateChanged, result.Data.Items[0].ActivityType);
    }

    [Fact]
    public async Task GetEarnings_AuthenticatedLawyer_Returns200WithEarningsSummary()
    {
        var lawyerId = Guid.NewGuid();
        var nowUtc = DateTimeOffset.UtcNow;

        await _factory.SeedUserAsync(lawyerId, $"lawyer_{lawyerId:N}@test.com", "Lawyer", "Test Lawyer Earnings");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var wallet = new LawyerWallet(Guid.NewGuid(), lawyerId, nowUtc.AddMonths(-1))
            {
                AvailableBalance = 9000m,
                PendingBalance = 1000m,
                Currency = "EGP"
            };
            db.LawyerWallets.Add(wallet);
            await db.SaveChangesAsync();
        }

        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var response = await lawyerClient.GetAsync("/api/lawyers/dashboard/earnings?period=6months&groupBy=monthly");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LawyerEarningsSummaryDto>>(JsonOptions);
        Assert.NotNull(result?.Data);
        Assert.Equal(9000m, result.Data.CurrentAvailableBalance);
        Assert.Equal(1000m, result.Data.CurrentPendingBalance);
        Assert.Equal("EGP", result.Data.Currency);
        Assert.NotEmpty(result.Data.PeriodBreakdown);
    }

    [Fact]
    public async Task GetUpcomingDeadlines_AuthenticatedLawyer_Returns200WithDeadlines()
    {
        var lawyerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var nowUtc = DateTimeOffset.UtcNow;

        await _factory.SeedUserAsync(lawyerId, $"lawyer_{lawyerId:N}@test.com", "Lawyer", "Test Lawyer Deadlines");
        await _factory.SeedUserAsync(clientId, $"client_{clientId:N}@test.com", "Client", "Test Client Deadlines");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var contract = new Contract(
                contractId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                clientId,
                lawyerId,
                "عقد استشارات شركات",
                "شروط العقد",
                nowUtc.AddDays(-10))
            {
                Status = ContractStatus.Active,
                Currency = "EGP"
            };
            db.Contracts.Add(contract);

            var milestone = new Milestone(
                Guid.NewGuid(),
                contractId,
                "مرحلة صياغة العقود",
                "صياغة كافة العقود",
                1,
                8000m,
                14,
                nowUtc.AddDays(4),
                nowUtc.AddDays(-10))
            {
                Status = MilestoneStatus.FundedInProgress
            };
            db.Milestones.Add(milestone);

            await db.SaveChangesAsync();
        }

        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var response = await lawyerClient.GetAsync("/api/lawyers/dashboard/upcoming-deadlines?daysAhead=30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<UpcomingDeadlineItemDto>>>(JsonOptions);
        Assert.NotNull(result?.Data);
        Assert.Single(result.Data);
        Assert.Equal("مرحلة صياغة العقود", result.Data[0].MilestoneTitle);
        Assert.Equal(DeadlineUrgency.Approaching, result.Data[0].Urgency);
    }

    [Fact]
    public async Task GetCalendarSchedule_AuthenticatedLawyer_Returns200WithCalendarSchedule()
    {
        var lawyerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var nowUtc = DateTimeOffset.UtcNow;

        await _factory.SeedUserAsync(lawyerId, $"lawyer_{lawyerId:N}@test.com", "Lawyer", "Test Lawyer Calendar");
        await _factory.SeedUserAsync(clientId, $"client_{clientId:N}@test.com", "Client", "Test Client Calendar");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var contract = new Contract(
                contractId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                clientId,
                lawyerId,
                "عقد استشارات عقارية",
                "شروط العقد",
                nowUtc.AddDays(-10))
            {
                Status = ContractStatus.Active,
                Currency = "EGP"
            };
            db.Contracts.Add(contract);

            var milestone = new Milestone(
                Guid.NewGuid(),
                contractId,
                "تسليم تقرير العقار",
                "تقرير مفصل عن العقار",
                1,
                6000m,
                7,
                nowUtc.AddDays(2),
                nowUtc.AddDays(-10))
            {
                Status = MilestoneStatus.FundedInProgress
            };
            db.Milestones.Add(milestone);

            await db.SaveChangesAsync();
        }

        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var response = await lawyerClient.GetAsync("/api/lawyers/dashboard/calendar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LawyerCalendarScheduleDto>>(JsonOptions);
        Assert.NotNull(result?.Data);
        Assert.Single(result.Data.Events);
        Assert.Equal(CalendarEventType.MilestoneDeadline, result.Data.Events[0].EventType);
    }

    [Fact]
    public async Task GetStats_Unauthenticated_Returns401Unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/lawyers/dashboard/stats");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
