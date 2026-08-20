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
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Ratings.DTOs;
using SmartCourt.Features.Ratings.Enums;
using SmartCourt.Persistence;
using SmartCourt.Tests.Common;
using Xunit;

namespace SmartCourt.Tests.Features.Ratings;

public class RatingApiE2ETests : IClassFixture<SmartCourtWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SmartCourtWebApplicationFactory _factory;

    public RatingApiE2ETests(SmartCourtWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(Guid LawyerId, Guid ClientId, Guid ContractId)> SeedCompletedContractAsync()
    {
        var lawyerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var legalCaseId = Guid.NewGuid();
        var contractId = Guid.NewGuid();

        await _factory.SeedUserAsync(lawyerId, $"lawyer_{lawyerId:N}@test.com", "Lawyer", "Test Lawyer");
        await _factory.SeedUserAsync(clientId, $"client_{clientId:N}@test.com", "Client", "Test Client");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var lawyerProfile = new LawyerProfile
        {
            UserId = lawyerId,
            AverageRating = 0m,
            TotalRatingSum = 0,
            TotalRatingCount = 0
        };
        db.LawyerProfiles.Add(lawyerProfile);

        var caseEntity = new SmartCourt.Entities.Case
        {
            Id = legalCaseId,
            ClientId = clientId,
            Title = "قضية تجارية",
            Description = "نزاع تجاري",
            City = "القاهرة",
            SubmittedAt = DateTime.UtcNow,
            Status = CaseStatus.Matched
        };

        var proposal = new Proposal(
            proposalId,
            legalCaseId,
            clientId,
            lawyerId,
            DateTime.UtcNow)
        {
            Status = ProposalStatus.Accepted
        };

        var contract = new Contract(
            contractId,
            proposalId,
            legalCaseId,
            clientId,
            lawyerId,
            "عقد استشارة تجارية",
            "شروط وأحكام العقد كافية للاختبار والتحقق.",
            DateTime.UtcNow.AddDays(-5))
        {
            Status = ContractStatus.Completed,
            CompletedAt = DateTime.UtcNow.AddDays(-1)
        };

        db.Cases.Add(caseEntity);
        db.Proposals.Add(proposal);
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();

        return (lawyerId, clientId, contractId);
    }

    private async Task<(Guid LawyerId, Guid ClientId, Guid ContractId)> SeedTerminatedContractAsync()
    {
        var lawyerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var legalCaseId = Guid.NewGuid();
        var contractId = Guid.NewGuid();

        await _factory.SeedUserAsync(lawyerId, $"lawyer_{lawyerId:N}@test.com", "Lawyer", "Test Lawyer Term");
        await _factory.SeedUserAsync(clientId, $"client_{clientId:N}@test.com", "Client", "Test Client Term");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var lawyerProfile = new LawyerProfile
        {
            UserId = lawyerId,
            AverageRating = 0m,
            TotalRatingSum = 0,
            TotalRatingCount = 0
        };
        db.LawyerProfiles.Add(lawyerProfile);

        var caseEntity = new SmartCourt.Entities.Case
        {
            Id = legalCaseId,
            ClientId = clientId,
            Title = "قضية عمالية",
            Description = "نزاع عمالي",
            City = "الإسكندرية",
            SubmittedAt = DateTime.UtcNow,
            Status = CaseStatus.Matched
        };

        var proposal = new Proposal(
            proposalId,
            legalCaseId,
            clientId,
            lawyerId,
            DateTime.UtcNow)
        {
            Status = ProposalStatus.Accepted
        };

        var contract = new Contract(
            contractId,
            proposalId,
            legalCaseId,
            clientId,
            lawyerId,
            "عقد استشارة عمالية",
            "شروط وأحكام العقد كافية للاختبار والتحقق.",
            DateTime.UtcNow.AddDays(-6))
        {
            Status = ContractStatus.Terminated,
            TerminatedAt = DateTime.UtcNow.AddDays(-2),
            TerminationReason = "تم تسوية النزاع بالتراضي"
        };

        db.Cases.Add(caseEntity);
        db.Proposals.Add(proposal);
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();

        return (lawyerId, clientId, contractId);
    }

    [Fact]
    public async Task FullRatingLifecycle_E2E_Success()
    {
        var (lawyerId, clientId, contractId) = await SeedCompletedContractAsync();

        var clientHttp = _factory.CreateAuthenticatedClient(clientId, "Client");
        var lawyerHttp = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        // 1. Client submits 5-star rating for lawyer
        var clientRatingRequest = new SubmitRatingRequest(5, "محامي ممتاز ومحترف جداً");
        var clientPostResponse = await clientHttp.PostAsJsonAsync(
            $"/api/contracts/{contractId}/ratings",
            clientRatingRequest,
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, clientPostResponse.StatusCode);
        var clientPostResult = await clientPostResponse.Content.ReadFromJsonAsync<ApiResponse<ContractRatingDto>>(JsonOptions);
        Assert.NotNull(clientPostResult?.Data);
        Assert.Equal(5, clientPostResult.Data.Stars);
        Assert.Equal(RaterRole.Client, clientPostResult.Data.RaterRole);

        // Verify LawyerProfile updated atomically in DB
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var profile = await db.LawyerProfiles.FirstAsync(p => p.UserId == lawyerId);
            Assert.Equal(1, profile.TotalRatingCount);
            Assert.Equal(5, profile.TotalRatingSum);
            Assert.Equal(5.00m, profile.AverageRating);
        }

        // 2. Sealed Envelope check before lawyer submits
        // Client sees their own rating
        var clientGetResponse = await clientHttp.GetAsync($"/api/contracts/{contractId}/ratings");
        Assert.Equal(HttpStatusCode.OK, clientGetResponse.StatusCode);
        var clientSummary = (await clientGetResponse.Content.ReadFromJsonAsync<ApiResponse<ContractRatingSummaryDto>>(JsonOptions))?.Data;
        Assert.NotNull(clientSummary);
        Assert.False(clientSummary.AreRevealed);
        Assert.NotNull(clientSummary.ClientRating);
        Assert.Null(clientSummary.LawyerRating);

        // Lawyer sees nothing yet (sealed envelope!)
        var lawyerGetResponse = await lawyerHttp.GetAsync($"/api/contracts/{contractId}/ratings");
        Assert.Equal(HttpStatusCode.OK, lawyerGetResponse.StatusCode);
        var lawyerSummary = (await lawyerGetResponse.Content.ReadFromJsonAsync<ApiResponse<ContractRatingSummaryDto>>(JsonOptions))?.Data;
        Assert.NotNull(lawyerSummary);
        Assert.False(lawyerSummary.AreRevealed);
        Assert.Null(lawyerSummary.ClientRating);
        Assert.Null(lawyerSummary.LawyerRating);

        // Public lawyer ratings query does not return unrevealed rating
        var lawyerRatingsBeforeResponse = await clientHttp.GetAsync($"/api/lawyers/{lawyerId}/ratings");
        Assert.Equal(HttpStatusCode.OK, lawyerRatingsBeforeResponse.StatusCode);
        var lawyerRatingsBefore = (await lawyerRatingsBeforeResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ContractRatingDto>>>(JsonOptions))?.Data;
        Assert.NotNull(lawyerRatingsBefore);
        Assert.Equal(0, lawyerRatingsBefore.TotalCount);

        // 3. Lawyer submits 4-star rating for client
        var lawyerRatingRequest = new SubmitRatingRequest(4, "عميل متعاون");
        var lawyerPostResponse = await lawyerHttp.PostAsJsonAsync(
            $"/api/contracts/{contractId}/ratings",
            lawyerRatingRequest,
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, lawyerPostResponse.StatusCode);

        // 4. Reveal check: now both ratings are visible to both parties!
        var clientRevealedResponse = await clientHttp.GetAsync($"/api/contracts/{contractId}/ratings");
        var clientRevealedSummary = (await clientRevealedResponse.Content.ReadFromJsonAsync<ApiResponse<ContractRatingSummaryDto>>(JsonOptions))?.Data;
        Assert.NotNull(clientRevealedSummary);
        Assert.True(clientRevealedSummary.AreRevealed);
        Assert.NotNull(clientRevealedSummary.ClientRating);
        Assert.Equal(5, clientRevealedSummary.ClientRating.Stars);
        Assert.NotNull(clientRevealedSummary.LawyerRating);
        Assert.Equal(4, clientRevealedSummary.LawyerRating.Stars);

        var lawyerRevealedResponse = await lawyerHttp.GetAsync($"/api/contracts/{contractId}/ratings");
        var lawyerRevealedSummary = (await lawyerRevealedResponse.Content.ReadFromJsonAsync<ApiResponse<ContractRatingSummaryDto>>(JsonOptions))?.Data;
        Assert.NotNull(lawyerRevealedSummary);
        Assert.True(lawyerRevealedSummary.AreRevealed);
        Assert.NotNull(lawyerRevealedSummary.ClientRating);
        Assert.NotNull(lawyerRevealedSummary.LawyerRating);

        // Public lawyer ratings query now includes revealed client rating
        var lawyerRatingsAfterResponse = await clientHttp.GetAsync($"/api/lawyers/{lawyerId}/ratings");
        Assert.Equal(HttpStatusCode.OK, lawyerRatingsAfterResponse.StatusCode);
        var lawyerRatingsAfter = (await lawyerRatingsAfterResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ContractRatingDto>>>(JsonOptions))?.Data;
        Assert.NotNull(lawyerRatingsAfter);
        Assert.Equal(1, lawyerRatingsAfter.TotalCount);
        Assert.Equal(5, lawyerRatingsAfter.Items[0].Stars);

        // 5. Attempt duplicate rating -> rejected with BadRequest
        var duplicateResponse = await clientHttp.PostAsJsonAsync(
            $"/api/contracts/{contractId}/ratings",
            clientRatingRequest,
            JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);

        // 6. Attempt invalid rating (0 or 6 stars) -> rejected with BadRequest
        var invalidResponse = await clientHttp.PostAsJsonAsync(
            $"/api/contracts/{contractId}/ratings",
            new SubmitRatingRequest(6),
            JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);
    }

    [Fact]
    public async Task TerminatedContract_CanBeRated_E2E()
    {
        var (lawyerId, clientId, contractId) = await SeedTerminatedContractAsync();

        var clientHttp = _factory.CreateAuthenticatedClient(clientId, "Client");
        var lawyerHttp = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var clientRequest = new SubmitRatingRequest(3, "تم إنهاء العقد بالاتفاق");
        var clientResponse = await clientHttp.PostAsJsonAsync(
            $"/api/contracts/{contractId}/ratings",
            clientRequest,
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, clientResponse.StatusCode);

        var lawyerRequest = new SubmitRatingRequest(3, "تم تسوية العقد");
        var lawyerResponse = await lawyerHttp.PostAsJsonAsync(
            $"/api/contracts/{contractId}/ratings",
            lawyerRequest,
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, lawyerResponse.StatusCode);

        var clientViewResponse = await clientHttp.GetAsync($"/api/contracts/{contractId}/ratings");
        var clientView = (await clientViewResponse.Content.ReadFromJsonAsync<ApiResponse<ContractRatingSummaryDto>>(JsonOptions))?.Data;
        Assert.NotNull(clientView);
        Assert.True(clientView.AreRevealed);
        Assert.NotNull(clientView.ClientRating);
        Assert.NotNull(clientView.LawyerRating);
        Assert.Equal(3, clientView.ClientRating.Stars);
        Assert.Equal(3, clientView.LawyerRating.Stars);
    }

    [Fact]
    public async Task Moderator_BypassesSealedEnvelope_E2E()
    {
        var (lawyerId, clientId, contractId) = await SeedCompletedContractAsync();

        var moderatorId = Guid.NewGuid();
        await _factory.SeedUserAsync(moderatorId, $"mod_{moderatorId:N}@test.com", "Moderator", "Test Moderator");

        var clientHttp = _factory.CreateAuthenticatedClient(clientId, "Client");
        var modHttp = _factory.CreateAuthenticatedClient(moderatorId, "Moderator");

        // Client rates lawyer
        await clientHttp.PostAsJsonAsync(
            $"/api/contracts/{contractId}/ratings",
            new SubmitRatingRequest(5, "سرّي"),
            JsonOptions);

        // Moderator views contract ratings -> sees the client's rating even though lawyer hasn't submitted
        var modResponse = await modHttp.GetAsync($"/api/contracts/{contractId}/ratings");
        Assert.Equal(HttpStatusCode.OK, modResponse.StatusCode);

        var summary = (await modResponse.Content.ReadFromJsonAsync<ApiResponse<ContractRatingSummaryDto>>(JsonOptions))?.Data;
        Assert.NotNull(summary);
        Assert.False(summary.AreRevealed);
        Assert.NotNull(summary.ClientRating);
        Assert.Equal(5, summary.ClientRating.Stars);
        Assert.Null(summary.LawyerRating);
    }

    [Fact]
    public async Task UpdateRating_SuccessfullyUpdatesRatingAndRecalculatesProfile_E2E()
    {
        var (lawyerId, clientId, contractId) = await SeedCompletedContractAsync();

        var clientHttp = _factory.CreateAuthenticatedClient(clientId, "Client");

        // 1. Initial rating
        var submitResponse = await clientHttp.PostAsJsonAsync(
            $"/api/contracts/{contractId}/ratings",
            new SubmitRatingRequest(3, "خدمة متوسطة"),
            JsonOptions);
        Assert.Equal(HttpStatusCode.Created, submitResponse.StatusCode);

        // 2. Update rating
        var updateRequest = new UpdateRatingRequest(5, "خدمة ممتازة بعد المتابعة");
        var updateResponse = await clientHttp.PutAsJsonAsync(
            $"/api/contracts/{contractId}/ratings",
            updateRequest,
            JsonOptions);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedDto = (await updateResponse.Content.ReadFromJsonAsync<ApiResponse<ContractRatingDto>>(JsonOptions))?.Data;
        Assert.NotNull(updatedDto);
        Assert.Equal(5, updatedDto.Stars);
        Assert.Equal("خدمة ممتازة بعد المتابعة", updatedDto.Comment);
        Assert.Equal("Test Client", updatedDto.RaterName);
        Assert.Equal("Test Lawyer", updatedDto.RatedName);


        // 3. Verify in database
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var lawyerProfile = await db.LawyerProfiles.FirstOrDefaultAsync(p => p.UserId == lawyerId);
        Assert.NotNull(lawyerProfile);
        Assert.Equal(1, lawyerProfile.TotalRatingCount);
        Assert.Equal(5, lawyerProfile.TotalRatingSum);
        Assert.Equal(5.00m, lawyerProfile.AverageRating);
    }
}

