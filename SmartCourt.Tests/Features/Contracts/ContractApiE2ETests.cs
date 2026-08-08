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
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Milestones.DTOs;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Persistence;
using SmartCourt.Tests.Common;
using Xunit;

namespace SmartCourt.Tests.Features.Contracts;

public class ContractApiE2ETests : IClassFixture<SmartCourtWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SmartCourtWebApplicationFactory _factory;

    public ContractApiE2ETests(SmartCourtWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> GetContractETagAsync(Guid contractId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var entity = await db.Contracts.FirstAsync(c => c.Id == contractId);
        return $"\"{Convert.ToBase64String(entity.RowVersion)}\"";
    }

    private async Task<(Guid LawyerId, Guid ClientId, Guid ProposalId, Guid LegalCaseId)> SeedProposalAndCaseAsync()
    {
        var lawyerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var legalCaseId = Guid.NewGuid();

        await _factory.SeedUserAsync(lawyerId, $"lawyer_{lawyerId:N}@test.com", "Lawyer", "Test Lawyer");
        await _factory.SeedUserAsync(clientId, $"client_{clientId:N}@test.com", "Client", "Test Client");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var caseEntity = new SmartCourt.Entities.Case { Id = legalCaseId, ClientId = clientId, Title = "قضية عقارية", Description = "نزاع ملكية عقارية", City = "القاهرة", SubmittedAt = DateTime.UtcNow, Status = CaseStatus.Matched };

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

        return (lawyerId, clientId, proposalId, legalCaseId);
    }

    [Fact]
    public async Task PostContract_HappyPath_Returns201CreatedWithApiResponse()
    {
        var (lawyerId, _, proposalId, _) = await SeedProposalAndCaseAsync();
        var client = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var request = new CreateContractRequest(
            proposalId,
            "عقد تمثيل قانوني",
            "الشروط والأحكام الكاملة الشاملة لجميع جوانب العمل والالتزامات القانونية.");

        var response = await client.PostAsJsonAsync("/api/contracts", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions);
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(request.Title, apiResponse.Data.Title);
        Assert.Equal(ContractStatus.Draft, apiResponse.Data.Status);
    }

    [Fact]
    public async Task PostContract_ValidationFailed_Returns400BadRequest()
    {
        var (lawyerId, _, _, _) = await SeedProposalAndCaseAsync();
        var client = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var request = new CreateContractRequest(
            Guid.Empty,
            "",
            "شروط قصيرة جدا");

        var response = await client.PostAsJsonAsync("/api/contracts", request);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PostContract_Unauthenticated_Returns401Unauthorized()
    {
        var client = _factory.CreateClient();
        var request = new CreateContractRequest(
            Guid.NewGuid(),
            "عنوان العقد",
            "شروط وأحكام العقد للتجربة الشاملة.");

        var response = await client.PostAsJsonAsync("/api/contracts", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostContract_ForbiddenRole_Returns403Forbidden()
    {
        var (_, clientId, proposalId, _) = await SeedProposalAndCaseAsync();
        var client = _factory.CreateAuthenticatedClient(clientId, "Client");

        var request = new CreateContractRequest(
            proposalId,
            "عقد تمثيل قانوني",
            "الشروط والأحكام الكاملة الشاملة للاتفاقية القانونية.");

        var response = await client.PostAsJsonAsync("/api/contracts", request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetContracts_HappyPath_Returns200OkWithPagedResult()
    {
        var (lawyerId, _, _, _) = await SeedProposalAndCaseAsync();
        var client = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var response = await client.GetAsync("/api/contracts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ContractSummaryDto>>>(JsonOptions);
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
    }

    [Fact]
    public async Task GetContractById_HappyPath_Returns200Ok()
    {
        var (lawyerId, _, proposalId, _) = await SeedProposalAndCaseAsync();
        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var createReq = new CreateContractRequest(
            proposalId,
            "عقد تفصيلي",
            "الشروط والأحكام الخاصة بالعقد التفصيلي الكامل الشامل لجميع الأطراف.");
        var createResp = await lawyerClient.PostAsJsonAsync("/api/contracts", createReq);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions);
        var contractId = created!.Data!.Id;

        var getResp = await lawyerClient.GetAsync($"/api/contracts/{contractId}");
        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        var getApiResponse = await getResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions);
        Assert.NotNull(getApiResponse);
        Assert.Equal(contractId, getApiResponse.Data!.Id);
    }

    [Fact]
    public async Task PutContract_HappyPath_UpdatesDraftWithETag()
    {
        var (lawyerId, _, proposalId, _) = await SeedProposalAndCaseAsync();
        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var createReq = new CreateContractRequest(
            proposalId,
            "عقد أولى",
            "الشروط والأحكام الخاصة بالعقد الأولية الشاملة لجميع الجوانب.");
        var createResp = await lawyerClient.PostAsJsonAsync("/api/contracts", createReq);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions);
        var contractId = created!.Data!.Id;

        var updateReq = new UpdateContractRequest(
            "عنوان عقد معدل",
            "الشروط والأحكام المعدلة للعقد بالشكل النهائي الشامل لكافة التفاصيل.");

        var etag = await GetContractETagAsync(contractId);
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/contracts/{contractId}")
        {
            Content = JsonContent.Create(updateReq)
        };
        requestMessage.Headers.TryAddWithoutValidation("If-Match", etag);

        var updateResp = await lawyerClient.SendAsync(requestMessage);
        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);
        var updateApiResponse = await updateResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions);
        Assert.NotNull(updateApiResponse);
        Assert.Equal(updateReq.Title, updateApiResponse.Data!.Title);
    }

    [Fact]
    public async Task PutContract_MissingETag_Returns400Or412()
    {
        var (lawyerId, _, proposalId, _) = await SeedProposalAndCaseAsync();
        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var createReq = new CreateContractRequest(
            proposalId,
            "عقد مسودة",
            "الشروط والأحكام المبدئية للعقد الشاملة للتجربة.");
        var createResp = await lawyerClient.PostAsJsonAsync("/api/contracts", createReq);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions);
        var contractId = created!.Data!.Id;

        var updateReq = new UpdateContractRequest(
            "عنوان عقد معدل بدون إيتاج",
            "الشروط والأحكام المعدلة للعقد بالشكل النهائي.");

        var updateResp = await lawyerClient.PutAsJsonAsync($"/api/contracts/{contractId}", updateReq);
        Assert.True(updateResp.StatusCode == HttpStatusCode.BadRequest || updateResp.StatusCode == HttpStatusCode.PreconditionFailed);
    }

    [Fact]
    public async Task AcceptContract_LawyerAndClientSign_TransitionsToActive()
    {
        var (lawyerId, clientId, proposalId, _) = await SeedProposalAndCaseAsync();
        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // 1. Create draft
        var createReq = new CreateContractRequest(
            proposalId,
            "عقد الاتفاق النهائى",
            "الشروط والأحكام الخاصة بالعقد للطرفين والشاملة لكافة بنود التعهدات.");
        var createResp = await lawyerClient.PostAsJsonAsync("/api/contracts", createReq);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions);
        var contractId = created!.Data!.Id;

        // 2. Add 1 milestone & approve by both so contract can activate upon signature
        var mReq = new AddMilestoneRequest("المرحلة 1", "وصف المرحلة", 1, 1000m, 5, DateTime.UtcNow.AddDays(5));
        var addMResp = await lawyerClient.PostAsJsonAsync($"/api/contracts/{contractId}/milestones", mReq);
        Assert.Equal(HttpStatusCode.Created, addMResp.StatusCode);
        var mDto = (await addMResp.Content.ReadFromJsonAsync<ApiResponse<MilestoneDto>>(JsonOptions))!.Data!;

        var mEntity = await db.Milestones.FirstAsync(m => m.Id == mDto.Id);
        var mEtag1 = $"\"{Convert.ToBase64String(mEntity.RowVersion)}\"";
        var appLMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{mDto.Id}/approve");
        appLMsg.Headers.TryAddWithoutValidation("If-Match", mEtag1);
        await lawyerClient.SendAsync(appLMsg);

        await db.Entry(mEntity).ReloadAsync();
        var mEtag2 = $"\"{Convert.ToBase64String(mEntity.RowVersion)}\"";
        var appCMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{mDto.Id}/approve");
        appCMsg.Headers.TryAddWithoutValidation("If-Match", mEtag2);
        await clientUserClient.SendAsync(appCMsg);

        // 3. Lawyer signs
        var lawyerSignReq = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{contractId}/accept");
        lawyerSignReq.Headers.TryAddWithoutValidation("If-Match", await GetContractETagAsync(contractId));
        var lawyerSignResp = await lawyerClient.SendAsync(lawyerSignReq);
        Assert.Equal(HttpStatusCode.OK, lawyerSignResp.StatusCode);
        var lawyerSignResult = await lawyerSignResp.Content.ReadFromJsonAsync<ApiResponse<ContractActionResultDto>>(JsonOptions);
        Assert.NotNull(lawyerSignResult);
        Assert.Equal("Draft", lawyerSignResult.Data!.Status);

        // 4. Client signs -> Transitions to Active
        var clientSignReq = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{contractId}/accept");
        clientSignReq.Headers.TryAddWithoutValidation("If-Match", await GetContractETagAsync(contractId));
        var clientSignResp = await clientUserClient.SendAsync(clientSignReq);
        Assert.Equal(HttpStatusCode.OK, clientSignResp.StatusCode);
        var clientSignResult = await clientSignResp.Content.ReadFromJsonAsync<ApiResponse<ContractActionResultDto>>(JsonOptions);
        Assert.NotNull(clientSignResult);
        Assert.Equal(ContractStatus.Active.ToString(), clientSignResult.Data!.Status);
    }

    [Fact]
    public async Task TerminateContract_HappyPath_TerminatesDraftContract()
    {
        var (lawyerId, _, proposalId, _) = await SeedProposalAndCaseAsync();
        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var createReq = new CreateContractRequest(
            proposalId,
            "عقد للإلغاء",
            "الشروط والأحكام الخاصة بالعقد قبل الإلغاء الشاملة للتفاصيل.");
        var createResp = await lawyerClient.PostAsJsonAsync("/api/contracts", createReq);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions);
        var contractId = created!.Data!.Id;

        var termReq = new TerminateContractRequest("تم إنهاء العقد باتفاق الطرفين قبل التوقيع.");
        var etag = await GetContractETagAsync(contractId);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{contractId}/terminate")
        {
            Content = JsonContent.Create(termReq)
        };
        requestMessage.Headers.TryAddWithoutValidation("If-Match", etag);

        var termResp = await lawyerClient.SendAsync(requestMessage);
        Assert.Equal(HttpStatusCode.OK, termResp.StatusCode);
        var termResult = await termResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions);
        Assert.NotNull(termResult);
        Assert.Equal(ContractStatus.Terminated, termResult.Data!.Status);
    }

    [Fact]
    public async Task GetStateHistory_HappyPath_ReturnsAuditTrail()
    {
        var (lawyerId, _, proposalId, _) = await SeedProposalAndCaseAsync();
        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var createReq = new CreateContractRequest(
            proposalId,
            "عقد تتبع الحالة",
            "الشروط والأحكام الخاصة بتتبع السجل التاريخي الشامل للتنفيذ.");
        var createResp = await lawyerClient.PostAsJsonAsync("/api/contracts", createReq);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions);
        var contractId = created!.Data!.Id;

        var historyResp = await lawyerClient.GetAsync($"/api/contracts/{contractId}/state-history");
        Assert.Equal(HttpStatusCode.OK, historyResp.StatusCode);
        var historyApiResponse = await historyResp.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ContractStateHistoryDto>>>(JsonOptions);
        Assert.NotNull(historyApiResponse);
        Assert.True(historyApiResponse.Success);
        Assert.NotEmpty(historyApiResponse.Data!.Items);
    }
}
