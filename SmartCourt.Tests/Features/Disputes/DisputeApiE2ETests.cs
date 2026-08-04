using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using SmartCourt.Features.Cases.Entities;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Disputes.DTOs;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Milestones.DTOs;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;
using SmartCourt.Tests.Common;
using Xunit;

namespace SmartCourt.Tests.Features.Disputes;

public class DisputeApiE2ETests : IClassFixture<SmartCourtWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SmartCourtWebApplicationFactory _factory;

    public DisputeApiE2ETests(SmartCourtWebApplicationFactory factory)
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

    private async Task<(Guid LawyerId, Guid ClientId, Guid ContractId, Guid MilestoneId)> SeedFundedMilestoneAsync()
    {
        var lawyerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var legalCaseId = Guid.NewGuid();

        await _factory.SeedUserAsync(lawyerId, $"lawyer_{lawyerId:N}@test.com", "Lawyer", "Test Lawyer");
        await _factory.SeedUserAsync(clientId, $"client_{clientId:N}@test.com", "Client", "Test Client");

        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");

        var fileId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var legalCase = new LegalCase(legalCaseId, clientId, "قضية نزاع", "نزاع تنفيذ", "القاهرة", DateTime.UtcNow) { Status = CaseStatus.Matched };
            var proposal = new Proposal(proposalId, legalCaseId, clientId, lawyerId, DateTime.UtcNow) { Status = ProposalStatus.Accepted };

            var storedFile = new StoredFile
            {
                Id = fileId,
                StoredFileName = "doc_test.pdf",
                OriginalFileName = "doc_test.pdf",
                FileUrl = "http://storage.test/doc_test.pdf",
                ContentType = "application/pdf",
                Extension = ".pdf",
                SizeInBytes = 1024
            };
            var verDoc = new UserVerificationDocument
            {
                Id = Guid.NewGuid(),
                UserId = lawyerId,
                StoredFileId = fileId,
                DocumentType = VerificationDocumentType.NationalIdFront,
                Status = VerificationDocumentStatus.Verified
            };

            db.LegalCases.Add(legalCase);
            db.Proposals.Add(proposal);
            db.StoredFiles.Add(storedFile);
            db.UserVerificationDocuments.Add(verDoc);
            await db.SaveChangesAsync();
        }

        var createReq = new CreateContractRequest(proposalId, "عقد النزاع", "الشروط الخاصة بالنزاع الكاملة الشاملة.");
        var createResp = await lawyerClient.PostAsJsonAsync("/api/contracts", createReq);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions);
        var contractId = created!.Data!.Id;

        var mReq = new AddMilestoneRequest("المرحلة المتنازع عليها", "تفاصيل العمل", 1, 3000m, 10, DateTime.UtcNow.AddDays(10));
        var mAddResp = await lawyerClient.PostAsJsonAsync($"/api/contracts/{contractId}/milestones", mReq);
        Assert.Equal(HttpStatusCode.Created, mAddResp.StatusCode);
        var m1Dto = (await mAddResp.Content.ReadFromJsonAsync<ApiResponse<MilestoneDto>>(JsonOptions))!.Data!;

        // Approve milestone by lawyer and client
        var appLMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/approve");
        appLMsg.Headers.TryAddWithoutValidation("If-Match", await GetMilestoneETagAsync(m1Dto.Id));
        await lawyerClient.SendAsync(appLMsg);

        var appCMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/approve");
        appCMsg.Headers.TryAddWithoutValidation("If-Match", await GetMilestoneETagAsync(m1Dto.Id));
        await clientUserClient.SendAsync(appCMsg);

        // Sign contract
        var sign1 = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{contractId}/accept");
        sign1.Headers.TryAddWithoutValidation("If-Match", await GetContractETagAsync(contractId));
        await lawyerClient.SendAsync(sign1);

        var sign2 = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{contractId}/accept");
        sign2.Headers.TryAddWithoutValidation("If-Match", await GetContractETagAsync(contractId));
        await clientUserClient.SendAsync(sign2);

        // Mark ready for funding
        var readyReq = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/ready-for-funding");
        readyReq.Headers.TryAddWithoutValidation("If-Match", await GetMilestoneETagAsync(m1Dto.Id));
        await lawyerClient.SendAsync(readyReq);

        // Fund milestone
        var fundReq = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/fund")
        {
            Content = JsonContent.Create(new FundMilestoneRequest("mock-success-card_12345"))
        };
        fundReq.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var fundResp = await clientUserClient.SendAsync(fundReq);
        Assert.Equal(HttpStatusCode.OK, fundResp.StatusCode);

        // Lawyer submits work with authorized StoredFileId
        var submitMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/submit")
        {
            Content = JsonContent.Create(new SubmitMilestoneRequest("تم تسليم الصحيفة للمراجعة كاملة.", [fileId]))
        };
        var submitResp = await lawyerClient.SendAsync(submitMsg);
        Assert.Equal(HttpStatusCode.OK, submitResp.StatusCode);

        // Client accepts submission -> AcceptedHold status
        var acceptSubResp = await clientUserClient.PostAsync($"/api/milestones/{m1Dto.Id}/accept", null);
        Assert.Equal(HttpStatusCode.OK, acceptSubResp.StatusCode);

        return (lawyerId, clientId, contractId, m1Dto.Id);
    }

    [Fact]
    public async Task CreateDispute_HappyPath_FreezesEscrowAndReturns201Created()
    {
        var (_, clientId, _, milestoneId) = await SeedFundedMilestoneAsync();
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");

        var createReq = new CreateDisputeRequest(
            milestoneId,
            DisputeCategory.DeliverableQuality,
            "نزاع جودة التسليم",
            "العمل المسلم لا يطابق المواصفات المتفق عليها في العقد.",
            DisputeRequestedOutcome.Refund,
            []);

        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/disputes")
        {
            Content = JsonContent.Create(createReq)
        };
        msg.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var response = await clientUserClient.SendAsync(msg);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var apiResp = await response.Content.ReadFromJsonAsync<ApiResponse<DisputeDto>>(JsonOptions);
        Assert.NotNull(apiResp);
        Assert.True(apiResp.Success);
        Assert.Equal(DisputeStatus.Open, apiResp.Data!.Status);
    }

    [Fact]
    public async Task CreateDispute_DuplicateIdempotencyKey_ReturnsSameDispute()
    {
        var (_, clientId, _, milestoneId) = await SeedFundedMilestoneAsync();
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");
        var idempotencyKey = Guid.NewGuid().ToString();

        var createReq = new CreateDisputeRequest(
            milestoneId,
            DisputeCategory.ContractTerms,
            "نزاع شروط العقد",
            "طلب العمل يتجاوز النطاق المحدد بالحالة.",
            DisputeRequestedOutcome.Refund,
            []);

        // Call 1
        var msg1 = new HttpRequestMessage(HttpMethod.Post, "/api/disputes") { Content = JsonContent.Create(createReq) };
        msg1.Headers.Add("Idempotency-Key", idempotencyKey);
        var resp1 = await clientUserClient.SendAsync(msg1);
        Assert.Equal(HttpStatusCode.Created, resp1.StatusCode);

        // Call 2 (Duplicate attempt without idempotency middleware header returns 400 BadRequest)
        var msg2 = new HttpRequestMessage(HttpMethod.Post, "/api/disputes") { Content = JsonContent.Create(createReq) };
        msg2.Headers.Add("Idempotency-Key", idempotencyKey);
        var resp2 = await clientUserClient.SendAsync(msg2);
        Assert.Equal(HttpStatusCode.BadRequest, resp2.StatusCode);
    }

    [Fact]
    public async Task AddEvidence_HappyPath_AddsDisputeEvidence()
    {
        var (lawyerId, clientId, _, milestoneId) = await SeedFundedMilestoneAsync();
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");

        var createReq = new CreateDisputeRequest(
            milestoneId,
            DisputeCategory.DeliverableQuality,
            "نزاع جودة العمل",
            "عدم جودة المستندات المقدمة.",
            DisputeRequestedOutcome.Refund,
            []);
        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/disputes") { Content = JsonContent.Create(createReq) };
        msg.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var disputeResp = await clientUserClient.SendAsync(msg);
        var disputeId = (await disputeResp.Content.ReadFromJsonAsync<ApiResponse<DisputeDto>>(JsonOptions))!.Data!.Id;

        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");
        var evidenceReq = new AddDisputeEvidenceRequest(
            "مستند إثبات تسليم العمل في الموعد المحدد مع الملاحظات كاملة.",
            []);

        var response = await lawyerClient.PostAsJsonAsync($"/api/disputes/{disputeId}/evidence", evidenceReq);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResp = await response.Content.ReadFromJsonAsync<ApiResponse<DisputeActionResultDto>>(JsonOptions);
        Assert.NotNull(apiResp);
        Assert.True(apiResp.Success);
    }

    [Fact]
    public async Task AdminDisputeLifecycle_AssignReviewResolveAndClose()
    {
        var adminId = Guid.NewGuid();
        await _factory.SeedUserAsync(adminId, "moderator@test.com", "Moderator");
        var moderatorClient = _factory.CreateAuthenticatedClient(adminId, "Moderator");

        var (_, clientId, _, milestoneId) = await SeedFundedMilestoneAsync();
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");

        var createReq = new CreateDisputeRequest(
            milestoneId,
            DisputeCategory.NonDelivery,
            "نزاع عدم التسليم",
            "لم يتم تسليم العمل مطلقًا من قبل المحامي.",
            DisputeRequestedOutcome.Refund,
            []);
        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/disputes") { Content = JsonContent.Create(createReq) };
        msg.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var disputeResp = await clientUserClient.SendAsync(msg);
        var disputeId = (await disputeResp.Content.ReadFromJsonAsync<ApiResponse<DisputeDto>>(JsonOptions))!.Data!.Id;

        // 1. Assign
        var assignReq = new AssignDisputeRequest(adminId);
        var assignResp = await moderatorClient.PostAsJsonAsync($"/api/admin/disputes/{disputeId}/assign", assignReq);
        Assert.Equal(HttpStatusCode.OK, assignResp.StatusCode);

        // 2. Review
        var reviewResp = await moderatorClient.PostAsync($"/api/admin/disputes/{disputeId}/review", null);
        Assert.Equal(HttpStatusCode.OK, reviewResp.StatusCode);

        // 3. Resolve (FullRefund)
        var resolveReq = new ResolveDisputeRequest(
            DisputeResolutionType.FullRefund,
            3000.00m,
            0.00m,
            "تسوية النزاع بإعادة المبلغ كاملاً للعميل.");

        var resolveMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/disputes/{disputeId}/resolve")
        {
            Content = JsonContent.Create(resolveReq)
        };
        resolveMsg.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var resolveResp = await moderatorClient.SendAsync(resolveMsg);
        Assert.Equal(HttpStatusCode.OK, resolveResp.StatusCode);

        // Mark OutboxMessage for DisputeResolved as Processed so CloseAsync can succeed
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var outboxMsg = await db.OutboxMessages.FirstAsync(o => o.AggregateId == disputeId && o.EventType == ContractPaymentEventTypes.DisputeResolved);
            outboxMsg.Status = OutboxStatus.Processed;
            await db.SaveChangesAsync();
        }

        // 4. Close
        var closeResp = await moderatorClient.PostAsync($"/api/admin/disputes/{disputeId}/close", null);
        Assert.Equal(HttpStatusCode.OK, closeResp.StatusCode);
        var finalData = (await closeResp.Content.ReadFromJsonAsync<ApiResponse<DisputeActionResultDto>>(JsonOptions))!.Data!;
        Assert.Equal(DisputeStatus.Closed.ToString(), finalData.Status);
    }
}
