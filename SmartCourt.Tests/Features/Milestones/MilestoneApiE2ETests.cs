using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Milestones.DTOs;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Persistence;
using SmartCourt.Tests.Common;
using Xunit;

namespace SmartCourt.Tests.Features.Milestones;

public class MilestoneApiE2ETests : IClassFixture<SmartCourtWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SmartCourtWebApplicationFactory _factory;

    public MilestoneApiE2ETests(SmartCourtWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(Guid LawyerId, Guid ClientId, Guid ContractId)> SeedActiveContractAsync()
    {
        var lawyerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var legalCaseId = Guid.NewGuid();

        await _factory.SeedUserAsync(lawyerId, $"lawyer_{lawyerId:N}@test.com", "Lawyer");
        await _factory.SeedUserAsync(clientId, $"client_{clientId:N}@test.com", "Client");

        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var caseEntity = new SmartCourt.Entities.Case { Id = legalCaseId, ClientId = clientId, Title = "قضية مالية", Description = "مطالبة مستحقات مالية", City = "القاهرة", SubmittedAt = DateTime.UtcNow, Status = CaseStatus.Matched };

        var proposal = new Proposal(
            proposalId,
            legalCaseId,
            clientId,
            lawyerId,
            DateTime.UtcNow)
        {
            Status = ProposalStatus.Accepted
        };

        db.Cases.Add(caseEntity);
        db.Proposals.Add(proposal);
        await db.SaveChangesAsync();

        // 1. Create contract draft
        var createReq = new CreateContractRequest(proposalId, "عقد مراحل الأعمال", "الشروط والأحكام الكاملة الشاملة لجميع جوانب العمل.");
        var createResp = await lawyerClient.PostAsJsonAsync("/api/contracts", createReq);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions);
        var contractId = created!.Data!.Id;

        // 2. Add 1 milestone in draft
        var m1Req = new AddMilestoneRequest("المرحلة الأولى: دراسة القضية", "تحليل المذكرات", 1, 1000m, 7, DateTime.UtcNow.AddDays(7));
        var addMResp = await lawyerClient.PostAsJsonAsync($"/api/contracts/{contractId}/milestones", m1Req);
        Assert.Equal(HttpStatusCode.Created, addMResp.StatusCode);
        var m1Dto = (await addMResp.Content.ReadFromJsonAsync<ApiResponse<MilestoneDto>>(JsonOptions))!.Data!;

        // 3. Both Lawyer and Client approve milestone m1 so it has AcceptedByLawyerAt & AcceptedByClientAt
        var m1Entity = await db.Milestones.FirstAsync(m => m.Id == m1Dto.Id);
        var m1EtagLawyer = $"\"{Convert.ToBase64String(m1Entity.RowVersion)}\"";

        var approveLawyerMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/approve");
        approveLawyerMsg.Headers.TryAddWithoutValidation("If-Match", m1EtagLawyer);
        var approveLawyerResp = await lawyerClient.SendAsync(approveLawyerMsg);
        Assert.Equal(HttpStatusCode.OK, approveLawyerResp.StatusCode);

        await db.Entry(m1Entity).ReloadAsync();
        var m1EtagClient = $"\"{Convert.ToBase64String(m1Entity.RowVersion)}\"";

        var approveClientMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/approve");
        approveClientMsg.Headers.TryAddWithoutValidation("If-Match", m1EtagClient);
        var approveClientResp = await clientUserClient.SendAsync(approveClientMsg);
        Assert.Equal(HttpStatusCode.OK, approveClientResp.StatusCode);

        // 4. Lawyer signs contract
        var contractEntity = await db.Contracts.FirstAsync(c => c.Id == contractId);
        var etag1 = $"\"{Convert.ToBase64String(contractEntity.RowVersion)}\"";

        var sign1 = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{contractId}/accept");
        sign1.Headers.TryAddWithoutValidation("If-Match", etag1);
        var sign1Resp = await lawyerClient.SendAsync(sign1);
        Assert.Equal(HttpStatusCode.OK, sign1Resp.StatusCode);

        // 5. Client signs contract -> Activates contract
        await db.Entry(contractEntity).ReloadAsync();
        var etag2 = $"\"{Convert.ToBase64String(contractEntity.RowVersion)}\"";

        var sign2 = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{contractId}/accept");
        sign2.Headers.TryAddWithoutValidation("If-Match", etag2);
        var sign2Resp = await clientUserClient.SendAsync(sign2);
        Assert.Equal(HttpStatusCode.OK, sign2Resp.StatusCode);

        return (lawyerId, clientId, contractId);
    }

    [Fact]
    public async Task AddMilestone_HappyPath_CreatesMilestoneInDraft()
    {
        var lawyerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var legalCaseId = Guid.NewGuid();

        await _factory.SeedUserAsync(lawyerId, $"lawyer_{lawyerId:N}@test.com", "Lawyer");
        await _factory.SeedUserAsync(clientId, $"client_{clientId:N}@test.com", "Client");

        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var caseEntity = new SmartCourt.Entities.Case { Id = legalCaseId, ClientId = clientId, Title = "قضية مسودة", Description = "وصف القضية", City = "القاهرة", SubmittedAt = DateTime.UtcNow, Status = CaseStatus.Matched };
        var proposal = new Proposal(proposalId, legalCaseId, clientId, lawyerId, DateTime.UtcNow) { Status = ProposalStatus.Accepted };
        db.Cases.Add(caseEntity);
        db.Proposals.Add(proposal);
        await db.SaveChangesAsync();

        var createReq = new CreateContractRequest(proposalId, "عقد مسودة للمراحل", "الشروط والأحكام العامة الشاملة لجميع جوانب العمل.");
        var createResp = await lawyerClient.PostAsJsonAsync("/api/contracts", createReq);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions);
        var contractId = created!.Data!.Id;

        var addMilestoneReq = new AddMilestoneRequest("المرحلة 1", "إعداد الصحيفة", 1, 5000m, 10, DateTime.UtcNow.AddDays(10));
        var addResp = await lawyerClient.PostAsJsonAsync($"/api/contracts/{contractId}/milestones", addMilestoneReq);

        Assert.Equal(HttpStatusCode.Created, addResp.StatusCode);
        var apiResp = await addResp.Content.ReadFromJsonAsync<ApiResponse<MilestoneDto>>(JsonOptions);
        Assert.NotNull(apiResp);
        Assert.True(apiResp.Success);
        Assert.Equal(addMilestoneReq.Title, apiResp.Data!.Title);
    }

    [Fact]
    public async Task AddMilestone_InvalidAmount_Returns400BadRequest()
    {
        var lawyerId = Guid.NewGuid();
        await _factory.SeedUserAsync(lawyerId, $"lawyer_{lawyerId:N}@test.com", "Lawyer");
        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var invalidReq = new AddMilestoneRequest("", "وصف", 1, -500m, -1, DateTime.UtcNow.AddDays(-5));
        var response = await lawyerClient.PostAsJsonAsync($"/api/contracts/{Guid.NewGuid()}/milestones", invalidReq);

        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ListMilestones_HappyPath_ReturnsMilestoneList()
    {
        var (lawyerId, _, contractId) = await SeedActiveContractAsync();
        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var response = await lawyerClient.GetAsync($"/api/contracts/{contractId}/milestones");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResp = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<MilestoneDto>>>(JsonOptions);
        Assert.NotNull(apiResp);
        Assert.True(apiResp.Success);
        Assert.NotEmpty(apiResp.Data!);
    }

    [Fact]
    public async Task MarkReadyForFunding_HappyPath_TransitionsToReadyForFunding()
    {
        var (lawyerId, _, contractId) = await SeedActiveContractAsync();
        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var listResp = await lawyerClient.GetAsync($"/api/contracts/{contractId}/milestones");
        Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);
        var milestones = (await listResp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<MilestoneDto>>>(JsonOptions))!.Data!;
        var m1 = milestones.First();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mEntity = await db.Milestones.FirstAsync(m => m.Id == m1.Id);
        var etag = $"\"{Convert.ToBase64String(mEntity.RowVersion)}\"";

        var readyReq = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1.Id}/ready-for-funding");
        readyReq.Headers.TryAddWithoutValidation("If-Match", etag);

        var readyResp = await lawyerClient.SendAsync(readyReq);
        Assert.Equal(HttpStatusCode.OK, readyResp.StatusCode);
        var apiResp = await readyResp.Content.ReadFromJsonAsync<ApiResponse<MilestoneDto>>(JsonOptions);
        Assert.NotNull(apiResp);
        Assert.Equal(MilestoneStatus.AwaitingFunding, apiResp.Data!.Status);
    }

    [Fact]
    public async Task ExpenseProposal_MidContractRequiresClientApprovalBeforeFunding()
    {
        var (lawyerId, clientId, contractId) =
            await SeedActiveContractAsync();
        var lawyerClient =
            _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");
        var clientUserClient =
            _factory.CreateAuthenticatedClient(clientId, "Client");
        var request = new AddMilestoneRequest(
            "Court filing fee",
            "Reimburse the paid filing fee.",
            null,
            2,
            750m,
            null,
            null,
            MilestoneType.Expense);

        var addResponse = await lawyerClient.PostAsJsonAsync(
            $"/api/contracts/{contractId}/milestones",
            request);

        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);
        var addJson = await addResponse.Content.ReadAsStringAsync();
        using (var document = JsonDocument.Parse(addJson))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.False(data.TryGetProperty("deliverables", out _));
            Assert.False(data.TryGetProperty("durationDays", out _));
        }

        var created = JsonSerializer.Deserialize<ApiResponse<MilestoneDto>>(
            addJson,
            JsonOptions)!.Data!;
        Assert.Equal(MilestoneType.Expense, created.Type);
        Assert.Equal(MilestoneStatus.Draft, created.Status);
        Assert.DoesNotContain("Fund", created.PermittedActions);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var entity = await db.Milestones.SingleAsync(
            milestone => milestone.Id == created.Id);
        var etag = $"\"{Convert.ToBase64String(entity.RowVersion)}\"";
        var approve = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/milestones/{created.Id}/approve");
        approve.Headers.TryAddWithoutValidation("If-Match", etag);

        var approveResponse = await clientUserClient.SendAsync(approve);

        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);
        var listResponse = await clientUserClient.GetAsync(
            $"/api/contracts/{contractId}/milestones");
        var approved = (await listResponse.Content
            .ReadFromJsonAsync<ApiResponse<IReadOnlyList<MilestoneDto>>>(
                JsonOptions))!.Data!.Single(
                    milestone => milestone.Id == created.Id);
        Assert.Equal(MilestoneStatus.AwaitingFunding, approved.Status);
        Assert.Contains("Fund", approved.PermittedActions);
        await db.Entry(entity).ReloadAsync();
        Assert.NotNull(entity.AcceptedByLawyerAt);
        Assert.NotNull(entity.AcceptedByClientAt);
        Assert.NotNull(entity.ReadyForFundingAt);
    }

    [Fact]
    public async Task ChangeRequestEndpoint_RemainsDisabled()
    {
        var (lawyerId, clientId, contractId) = await SeedActiveContractAsync();
        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");

        var listResp = await lawyerClient.GetAsync($"/api/contracts/{contractId}/milestones");
        Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);
        var milestones = (await listResp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<MilestoneDto>>>(JsonOptions))!.Data!;
        var m1 = milestones.First();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mEntity = await db.Milestones.FirstAsync(m => m.Id == m1.Id);
        var etag = $"\"{Convert.ToBase64String(mEntity.RowVersion)}\"";

        // Mark ready for funding
        var readyReq = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1.Id}/ready-for-funding");
        readyReq.Headers.TryAddWithoutValidation("If-Match", etag);
        var readyResp = await lawyerClient.SendAsync(readyReq);
        Assert.Equal(HttpStatusCode.OK, readyResp.StatusCode);

        // Client funds milestone
        var fundReq = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1.Id}/fund")
        {
            Content = JsonContent.Create(new SmartCourt.Features.Payments.DTOs.FundMilestoneRequest("mock-success-card_12345"))
        };
        fundReq.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var fundResp = await clientUserClient.SendAsync(fundReq);
        Assert.Equal(HttpStatusCode.OK, fundResp.StatusCode);

        // Create Change Request
        await db.Entry(mEntity).ReloadAsync();
        var mEtagForCr = $"\"{Convert.ToBase64String(mEntity.RowVersion)}\"";

        var crReq = new CreateMilestoneChangeRequest(
            ProposedDescription: "وصف مرحلة ممتدة",
            ProposedDurationDays: 14,
            ProposedDueDate: DateTime.UtcNow.AddDays(14),
            Reason: "طلب تمديد الوقت لظروف إضافية.");

        var crMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1.Id}/change-requests")
        {
            Content = JsonContent.Create(crReq)
        };
        crMsg.Headers.TryAddWithoutValidation("If-Match", mEtagForCr);

        var crResp = await lawyerClient.SendAsync(crMsg);
        Assert.Equal(HttpStatusCode.NotFound, crResp.StatusCode);
    }
}
