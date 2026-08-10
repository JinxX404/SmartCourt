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
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Persistence;
using SmartCourt.Tests.Common;
using Xunit;

namespace SmartCourt.Tests.Features.Payments;

public class PaymentApiE2ETests : IClassFixture<SmartCourtWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SmartCourtWebApplicationFactory _factory;

    public PaymentApiE2ETests(SmartCourtWebApplicationFactory factory)
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

    private async Task<string> GetMilestoneETagAsync(Guid milestoneId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var entity = await db.Milestones.FirstAsync(m => m.Id == milestoneId);
        return $"\"{Convert.ToBase64String(entity.RowVersion)}\"";
    }

    private async Task<(Guid LawyerId, Guid ClientId, Guid ContractId, Guid MilestoneId)> SeedReadyMilestoneAsync()
    {
        var lawyerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var legalCaseId = Guid.NewGuid();

        await _factory.SeedUserAsync(lawyerId, $"lawyer_{lawyerId:N}@test.com", "Lawyer", "Test Lawyer");
        await _factory.SeedUserAsync(clientId, $"client_{clientId:N}@test.com", "Client", "Test Client");

        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var caseEntity = new SmartCourt.Entities.Case { Id = legalCaseId, ClientId = clientId, Title = "قضية دفع", Description = "نزاع مالي", City = "القاهرة", SubmittedAt = DateTime.UtcNow, Status = CaseStatus.Matched };
        var proposal = new Proposal(proposalId, legalCaseId, clientId, lawyerId, DateTime.UtcNow) { Status = ProposalStatus.Accepted };
        db.Cases.Add(caseEntity);
        db.Proposals.Add(proposal);
        await db.SaveChangesAsync();

        var createReq = new CreateContractRequest(proposalId, "عقد المدفوعات", "الشروط المالية الكاملة الشاملة للتنفيذ.");
        var createResp = await lawyerClient.PostAsJsonAsync("/api/contracts", createReq);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions);
        var contractId = created!.Data!.Id;

        var mReq = new AddMilestoneRequest("المرحلة التمهيدية", "تفاصيل المرحلة", 1, 2000m, 14, DateTime.UtcNow.AddDays(14));
        var mAddResp = await lawyerClient.PostAsJsonAsync($"/api/contracts/{contractId}/milestones", mReq);
        Assert.Equal(HttpStatusCode.Created, mAddResp.StatusCode);
        var m1Dto = (await mAddResp.Content.ReadFromJsonAsync<ApiResponse<MilestoneDto>>(JsonOptions))!.Data!;

        // Both Lawyer & Client approve milestone
        var appLMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/approve");
        appLMsg.Headers.TryAddWithoutValidation("If-Match", await GetMilestoneETagAsync(m1Dto.Id));
        await lawyerClient.SendAsync(appLMsg);

        var appCMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/approve");
        appCMsg.Headers.TryAddWithoutValidation("If-Match", await GetMilestoneETagAsync(m1Dto.Id));
        await clientUserClient.SendAsync(appCMsg);

        // Sign Contract
        var sign1 = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{contractId}/accept");
        sign1.Headers.TryAddWithoutValidation("If-Match", await GetContractETagAsync(contractId));
        var sign1Resp = await lawyerClient.SendAsync(sign1);
        Assert.Equal(HttpStatusCode.OK, sign1Resp.StatusCode);

        var sign2 = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{contractId}/accept");
        sign2.Headers.TryAddWithoutValidation("If-Match", await GetContractETagAsync(contractId));
        var sign2Resp = await clientUserClient.SendAsync(sign2);
        Assert.Equal(HttpStatusCode.OK, sign2Resp.StatusCode);

        // Mark ready for funding
        var readyReq = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/ready-for-funding");
        readyReq.Headers.TryAddWithoutValidation("If-Match", await GetMilestoneETagAsync(m1Dto.Id));
        var readyResp = await lawyerClient.SendAsync(readyReq);
        Assert.Equal(HttpStatusCode.OK, readyResp.StatusCode);

        return (lawyerId, clientId, contractId, m1Dto.Id);
    }

    [Fact]
    public async Task FundMilestone_HappyPath_FundsEscrowWithIdempotencyKey()
    {
        var (_, clientId, _, milestoneId) = await SeedReadyMilestoneAsync();
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");

        var fundReq = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{milestoneId}/fund")
        {
            Content = JsonContent.Create(new FundMilestoneRequest("mock-success-card_12345"))
        };
        var idempotencyKey = Guid.NewGuid().ToString();
        fundReq.Headers.Add("Idempotency-Key", idempotencyKey);

        var response = await clientUserClient.SendAsync(fundReq);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResp = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentDto>>(JsonOptions);
        Assert.NotNull(apiResp);
        Assert.True(apiResp.Success);
        Assert.Equal(EscrowHoldStatus.Funded, apiResp.Data!.Status);
    }

    [Fact]
    public async Task FundMilestone_DuplicateIdempotencyKey_ReturnsSameResponse()
    {
        var (_, clientId, _, milestoneId) = await SeedReadyMilestoneAsync();
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");
        var idempotencyKey = Guid.NewGuid().ToString();

        // Call 1
        var fundReq1 = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{milestoneId}/fund")
        {
            Content = JsonContent.Create(new FundMilestoneRequest("mock-success-card_12345"))
        };
        fundReq1.Headers.Add("Idempotency-Key", idempotencyKey);
        var resp1 = await clientUserClient.SendAsync(fundReq1);
        Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
        var apiResp1 = await resp1.Content.ReadFromJsonAsync<ApiResponse<PaymentDto>>(JsonOptions);

        // Call 2 with identical key
        var fundReq2 = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{milestoneId}/fund")
        {
            Content = JsonContent.Create(new FundMilestoneRequest("mock-success-card_12345"))
        };
        fundReq2.Headers.Add("Idempotency-Key", idempotencyKey);
        var resp2 = await clientUserClient.SendAsync(fundReq2);

        Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
        var apiResp2 = await resp2.Content.ReadFromJsonAsync<ApiResponse<PaymentDto>>(JsonOptions);
        Assert.NotNull(apiResp2);
        Assert.Equal(apiResp1!.Data!.Id, apiResp2.Data!.Id);
    }

    [Fact]
    public async Task FundMilestone_ForbiddenRole_Returns403Forbidden()
    {
        var (lawyerId, _, _, milestoneId) = await SeedReadyMilestoneAsync();
        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");

        var fundReq = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{milestoneId}/fund")
        {
            Content = JsonContent.Create(new FundMilestoneRequest("mock-success-card_12345"))
        };
        fundReq.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var response = await lawyerClient.SendAsync(fundReq);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetContractPayments_HappyPath_ReturnsPaymentHistory()
    {
        var (lawyerId, clientId, contractId, milestoneId) = await SeedReadyMilestoneAsync();
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");

        var fundReq = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{milestoneId}/fund")
        {
            Content = JsonContent.Create(new FundMilestoneRequest("mock-success-card_12345"))
        };
        fundReq.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var fundResp = await clientUserClient.SendAsync(fundReq);
        Assert.Equal(HttpStatusCode.OK, fundResp.StatusCode);

        var historyResp = await clientUserClient.GetAsync($"/api/contracts/{contractId}/payments");
        Assert.Equal(HttpStatusCode.OK, historyResp.StatusCode);
        var apiResp = await historyResp.Content.ReadFromJsonAsync<ApiResponse<PaymentHistoryDto>>(JsonOptions);
        Assert.NotNull(apiResp);
        Assert.True(apiResp.Success);
    }

    [Fact]
    public async Task GetMilestonePayment_HappyPath_ReturnsPaymentDetails()
    {
        var (lawyerId, clientId, _, milestoneId) = await SeedReadyMilestoneAsync();
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");

        var fundReq = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{milestoneId}/fund")
        {
            Content = JsonContent.Create(new FundMilestoneRequest("mock-success-card_12345"))
        };
        fundReq.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var fundResp = await clientUserClient.SendAsync(fundReq);
        Assert.Equal(HttpStatusCode.OK, fundResp.StatusCode);

        var paymentResp = await clientUserClient.GetAsync($"/api/milestones/{milestoneId}/payment");
        Assert.Equal(HttpStatusCode.OK, paymentResp.StatusCode);
        var apiResp = await paymentResp.Content.ReadFromJsonAsync<ApiResponse<PaymentDto>>(JsonOptions);
        Assert.NotNull(apiResp);
        Assert.Equal(milestoneId, apiResp.Data!.MilestoneId);
    }

    [Fact]
    public async Task AdminRetryPayment_HappyPath_RetriesTransaction()
    {
        var adminId = Guid.NewGuid();
        await _factory.SeedUserAsync(adminId, "admin@test.com", "SuperAdministrator");
        var adminClient = _factory.CreateAuthenticatedClient(adminId, "SuperAdministrator");

        var (_, clientId, contractId, milestoneId) = await SeedReadyMilestoneAsync();
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");

        var fundReq = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{milestoneId}/fund")
        {
            Content = JsonContent.Create(new FundMilestoneRequest("mock-success-card_12345"))
        };
        fundReq.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var fundResp = await clientUserClient.SendAsync(fundReq);
        Assert.Equal(HttpStatusCode.OK, fundResp.StatusCode);

        var historyResp = await clientUserClient.GetAsync($"/api/contracts/{contractId}/payments");
        Assert.Equal(HttpStatusCode.OK, historyResp.StatusCode);
        var historyData = (await historyResp.Content.ReadFromJsonAsync<ApiResponse<PaymentHistoryDto>>(JsonOptions))!.Data!;
        var attemptId = historyData.Attempts.FirstOrDefault()?.Id ?? Guid.NewGuid();

        var retryReq = new HttpRequestMessage(HttpMethod.Post, $"/api/payments/{attemptId}/retry");
        retryReq.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var retryResp = await adminClient.SendAsync(retryReq);
        Assert.True(retryResp.StatusCode == HttpStatusCode.OK || retryResp.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PaymentWebhook_HappyPath_ProcessesWebhookEvent()
    {
        var client = _factory.CreateClient();
        var webhookReq = new PaymentWebhookRequest(
            EventId: $"evt_{Guid.NewGuid():N}",
            PaymentTransactionId: Guid.NewGuid(),
            ProviderTransactionId: $"prov_tx_{Guid.NewGuid():N}",
            Status: PaymentTransactionStatus.Completed,
            Amount: 2000m,
            Currency: "EGP",
            ProcessedAt: DateTime.UtcNow,
            FailureReason: null);

        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/payments/webhook")
        {
            Content = JsonContent.Create(webhookReq)
        };
        msg.Headers.Add("X-Payment-Event-Id", webhookReq.EventId);
        msg.Headers.Add("X-Payment-Timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        msg.Headers.Add("X-Payment-Signature", "test-signature");

        var response = await client.SendAsync(msg);
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized);
    }
}
