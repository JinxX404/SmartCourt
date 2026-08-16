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
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Disputes.DTOs;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Milestones.DTOs;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;
using SmartCourt.Tests.Common;
using Xunit;

namespace SmartCourt.Tests.Features.Contracts;

public class ContractToDisputeLifecycleE2ETests : IClassFixture<SmartCourtWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SmartCourtWebApplicationFactory _factory;

    public ContractToDisputeLifecycleE2ETests(SmartCourtWebApplicationFactory factory)
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

    [Fact]
    public async Task CompleteRealWorldClientLifecycle_FromContractDraftToDisputeResolution()
    {
        // -------------------------------------------------------------
        // SETUP & IDENTITY SEEDING
        // -------------------------------------------------------------
        var lawyerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();

        var proposalId = Guid.NewGuid();
        var legalCaseId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        await _factory.SeedUserAsync(lawyerId, $"lawyer_e2e_{lawyerId:N}@test.com", "Lawyer", "المحامي أحمد علي");
        await _factory.SeedUserAsync(clientId, $"client_e2e_{clientId:N}@test.com", "Client", "العميل محمد حسن");
        await _factory.SeedUserAsync(moderatorId, $"moderator_e2e_{moderatorId:N}@test.com", "Moderator", "المشرف المستشار سامي");

        var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");
        var clientUserClient = _factory.CreateAuthenticatedClient(clientId, "Client");
        var moderatorClient = _factory.CreateAuthenticatedClient(moderatorId, "Moderator");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var caseEntity = new SmartCourt.Entities.Case { Id = legalCaseId, ClientId = clientId, Title = "نزاع تجاري وعقاري", Description = "مطالبة بتنفيذ بند شرط جزائي وتصفية حسابات شركة عقارية", City = "الجيزة", SubmittedAt = DateTimeOffset.UtcNow, Status = CaseStatus.Matched };

            var proposal = new Proposal(
                proposalId,
                legalCaseId,
                clientId,
                lawyerId,
                DateTimeOffset.UtcNow)
            {
                Status = ProposalStatus.Accepted
            };

            var storedFile = new StoredFile
            {
                Id = fileId,
                StoredFileName = "work_doc.pdf",
                OriginalFileName = "work_doc.pdf",
                FileUrl = "http://storage.test/work_doc.pdf",
                ContentType = "application/pdf",
                Extension = ".pdf",
                SizeInBytes = 2048
            };
            db.Cases.Add(caseEntity);
            db.Proposals.Add(proposal);
            db.StoredFiles.Add(storedFile);
            await db.SaveChangesAsync();
        }

        // =============================================================
        // PHASE 1: CONTRACT MANAGEMENT (Create, Update, Accept)
        // =============================================================
        // Step 1: Lawyer creates contract draft
        var createContractReq = new CreateContractRequest(
            proposalId,
            "عقد تمثيل قانوني وإدارة نزاع تجاري",
            "الشروط والأحكام الكاملة الشاملة لجميع جوانب العمل والالتزامات القانونية.");

        var createContractResp = await lawyerClient.PostAsJsonAsync("/api/contracts", createContractReq);
        Assert.Equal(HttpStatusCode.Created, createContractResp.StatusCode);
        var contractData = (await createContractResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions))!.Data!;
        var contractId = contractData.Id;
        Assert.Equal(ContractStatus.Draft, contractData.Status);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            db.ContractAttachments.Add(
                new ContractAttachment(
                    Guid.NewGuid(),
                    contractId,
                    fileId,
                    lawyerId,
                    DateTimeOffset.UtcNow));
            await db.SaveChangesAsync();
        }

        // Step 2: Lawyer updates draft contract terms with If-Match ETag
        var updateContractReq = new UpdateContractRequest(
            "عقد تمثيل قانوني وإدارة نزاع تجاري (محدث)",
            "يتعهد المحامي بتمثيل العميل أمام كافة المحاكم الابتدائية والاستئنافية وتوفير الاستشارات الشاملة.");

        var updateMsg = new HttpRequestMessage(HttpMethod.Put, $"/api/contracts/{contractId}")
        {
            Content = JsonContent.Create(updateContractReq)
        };
        updateMsg.Headers.TryAddWithoutValidation("If-Match", await GetContractETagAsync(contractId));
        var updateResp = await lawyerClient.SendAsync(updateMsg);
        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);
        var updatedContract = (await updateResp.Content.ReadFromJsonAsync<ApiResponse<ContractDetailDto>>(JsonOptions))!.Data!;
        Assert.Equal(updateContractReq.Title, updatedContract.Title);

        // =============================================================
        // PHASE 2: MILESTONES LIFECYCLE (Add Milestones, Approve, Accept)
        // =============================================================
        // Step 3: Lawyer adds 2 Milestones
        var m1Req = new AddMilestoneRequest("المرحلة 1: إعداد صحيفة الدعوى", "دراسة المستندات وإيداع الصحيفة بالمحكمة", 1, 5000m, 10, DateTimeOffset.UtcNow.AddDays(10));
        var addM1Resp = await lawyerClient.PostAsJsonAsync($"/api/contracts/{contractId}/milestones", m1Req);
        Assert.Equal(HttpStatusCode.Created, addM1Resp.StatusCode);
        var m1Dto = (await addM1Resp.Content.ReadFromJsonAsync<ApiResponse<MilestoneDto>>(JsonOptions))!.Data!;

        var m2Req = new AddMilestoneRequest("المرحلة 2: المرافعة وتقديم المذكرات", "حضور الجلسات وتلخيص الدفوع القانونية", 2, 10000m, 30, DateTimeOffset.UtcNow.AddDays(40));
        var addM2Resp = await lawyerClient.PostAsJsonAsync($"/api/contracts/{contractId}/milestones", m2Req);
        Assert.Equal(HttpStatusCode.Created, addM2Resp.StatusCode);
        var m2Dto = (await addM2Resp.Content.ReadFromJsonAsync<ApiResponse<MilestoneDto>>(JsonOptions))!.Data!;

        // Step 4: Both Lawyer & Client approve milestone m1
        var approveM1LawyerMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/approve");
        approveM1LawyerMsg.Headers.TryAddWithoutValidation("If-Match", await GetMilestoneETagAsync(m1Dto.Id));
        var approveM1LawyerResp = await lawyerClient.SendAsync(approveM1LawyerMsg);
        Assert.Equal(HttpStatusCode.OK, approveM1LawyerResp.StatusCode);

        var approveM1ClientMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/approve");
        approveM1ClientMsg.Headers.TryAddWithoutValidation("If-Match", await GetMilestoneETagAsync(m1Dto.Id));
        var approveM1ClientResp = await clientUserClient.SendAsync(approveM1ClientMsg);
        Assert.Equal(HttpStatusCode.OK, approveM1ClientResp.StatusCode);

        // Step 5: Both Lawyer & Client approve milestone m2
        var approveM2LawyerMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m2Dto.Id}/approve");
        approveM2LawyerMsg.Headers.TryAddWithoutValidation("If-Match", await GetMilestoneETagAsync(m2Dto.Id));
        var approveM2LawyerResp = await lawyerClient.SendAsync(approveM2LawyerMsg);
        Assert.Equal(HttpStatusCode.OK, approveM2LawyerResp.StatusCode);

        var approveM2ClientMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m2Dto.Id}/approve");
        approveM2ClientMsg.Headers.TryAddWithoutValidation("If-Match", await GetMilestoneETagAsync(m2Dto.Id));
        var approveM2ClientResp = await clientUserClient.SendAsync(approveM2ClientMsg);
        Assert.Equal(HttpStatusCode.OK, approveM2ClientResp.StatusCode);

        // Step 6: Lawyer signs contract -> LawyerSigned
        var lawyerSignMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{contractId}/accept");
        lawyerSignMsg.Headers.TryAddWithoutValidation("If-Match", await GetContractETagAsync(contractId));
        var lawyerSignResp = await lawyerClient.SendAsync(lawyerSignMsg);
        Assert.Equal(HttpStatusCode.OK, lawyerSignResp.StatusCode);
        var lawyerSignData = (await lawyerSignResp.Content.ReadFromJsonAsync<ApiResponse<ContractActionResultDto>>(JsonOptions))!.Data!;
        Assert.Equal(ContractStatus.Draft.ToString(), lawyerSignData.Status);

        // Step 7: Client signs contract -> Active
        var clientSignMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{contractId}/accept");
        clientSignMsg.Headers.TryAddWithoutValidation("If-Match", await GetContractETagAsync(contractId));
        var clientSignResp = await clientUserClient.SendAsync(clientSignMsg);
        Assert.Equal(HttpStatusCode.OK, clientSignResp.StatusCode);
        var clientSignData = (await clientSignResp.Content.ReadFromJsonAsync<ApiResponse<ContractActionResultDto>>(JsonOptions))!.Data!;
        Assert.Equal(ContractStatus.Active.ToString(), clientSignData.Status);

        // =============================================================
        // PHASE 3: PAYMENTS & ESCROW (Fund Milestone 1)
        // =============================================================
        // Step 8: Lawyer marks Milestone 1 Ready for Funding
        var readyM1Msg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/ready-for-funding");
        readyM1Msg.Headers.TryAddWithoutValidation("If-Match", await GetMilestoneETagAsync(m1Dto.Id));
        var readyM1Resp = await lawyerClient.SendAsync(readyM1Msg);
        Assert.Equal(HttpStatusCode.OK, readyM1Resp.StatusCode);

        // Step 9: Client Funds Escrow for Milestone 1 with Idempotency Key
        var fundM1Msg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/fund")
        {
            Content = JsonContent.Create(new FundMilestoneRequest("mock-success-card_12345"))
        };
        var fundIdempotencyKey = Guid.NewGuid().ToString();
        fundM1Msg.Headers.Add("Idempotency-Key", fundIdempotencyKey);
        var fundM1Resp = await clientUserClient.SendAsync(fundM1Msg);
        Assert.Equal(HttpStatusCode.OK, fundM1Resp.StatusCode);
        var fundOperation = (await fundM1Resp.Content.ReadFromJsonAsync<ApiResponse<FundingOperationDto>>(JsonOptions))!.Data!;
        var fundData = Assert.IsType<PaymentDto>(fundOperation.Payment);
        Assert.Equal(EscrowHoldStatus.Funded, fundData.Status);

        // Step 10: Lawyer Submits Work for Milestone 1 with authorized fileId
        var submitReq = new SubmitMilestoneRequest("تم إعداد وإيداع الصحيفة بالمحكمة بنجاح ورقم القضية 1234 لسنة 2026.", [fileId]);
        var submitMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/milestones/{m1Dto.Id}/submit")
        {
            Content = JsonContent.Create(submitReq)
        };
        var submitResp = await lawyerClient.SendAsync(submitMsg);
        Assert.Equal(HttpStatusCode.OK, submitResp.StatusCode);

        // Step 10b: Client Accepts Submission -> Milestone transitions to AcceptedHold state
        var acceptSubResp = await clientUserClient.PostAsync($"/api/milestones/{m1Dto.Id}/accept", null);
        Assert.Equal(HttpStatusCode.OK, acceptSubResp.StatusCode);

        // =============================================================
        // PHASE 4: DISPUTE MANAGEMENT & ESCROW FREEZE / RESOLUTION
        // =============================================================
        // Step 11: Client opens dispute on Milestone 1 -> Escrow Frozen
        var createDisputeReq = new CreateDisputeRequest(
            m1Dto.Id,
            DisputeCategory.DeliverableQuality,
            "نزاع جودة التغطية الصحفية",
            "لم يتم التنسيق معي قبل إيداع الصحيفة وتوجد أخطاء في اسم الشركة المدعى عليها.",
            DisputeRequestedOutcome.Refund,
            []);

        var disputeMsg = new HttpRequestMessage(HttpMethod.Post, "/api/disputes")
        {
            Content = JsonContent.Create(createDisputeReq)
        };
        disputeMsg.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var disputeResp = await clientUserClient.SendAsync(disputeMsg);
        Assert.Equal(HttpStatusCode.Created, disputeResp.StatusCode);
        var disputeData = (await disputeResp.Content.ReadFromJsonAsync<ApiResponse<DisputeDto>>(JsonOptions))!.Data!;
        var disputeId = disputeData.Id;
        Assert.Equal(DisputeStatus.Open, disputeData.Status);

        // Step 12: Lawyer adds evidence to dispute
        var addEvidenceReq = new AddDisputeEvidenceRequest(
            "مسودة الصحيفة المعتمدة من العميل عبر الرسائل والملاحظات كاملة.",
            []);
        var evidenceResp = await lawyerClient.PostAsJsonAsync($"/api/disputes/{disputeId}/evidence", addEvidenceReq);
        Assert.Equal(HttpStatusCode.OK, evidenceResp.StatusCode);

        // Step 13: Admin/Moderator assigns dispute to themselves
        var assignReq = new AssignDisputeRequest(moderatorId);
        var assignResp = await moderatorClient.PostAsJsonAsync($"/api/admin/disputes/{disputeId}/assign", assignReq);
        Assert.Equal(HttpStatusCode.OK, assignResp.StatusCode);

        // Step 14: Admin puts dispute UnderReview
        var reviewResp = await moderatorClient.PostAsync($"/api/admin/disputes/{disputeId}/review", null);
        Assert.Equal(HttpStatusCode.OK, reviewResp.StatusCode);

        // Step 15: Admin resolves dispute with FullRefund (5000m refund to client)
        var resolveReq = new ResolveDisputeRequest(
            DisputeResolutionType.FullRefund,
            5000.00m,
            0.00m,
            "تقرر تسوية النزاع بإعادة مبلغ الضمان كاملاً للعميل.");

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

        // Step 16: Admin closes dispute
        var closeResp = await moderatorClient.PostAsync($"/api/admin/disputes/{disputeId}/close", null);
        Assert.Equal(HttpStatusCode.OK, closeResp.StatusCode);
        var finalDisputeData = (await closeResp.Content.ReadFromJsonAsync<ApiResponse<DisputeActionResultDto>>(JsonOptions))!.Data!;
        Assert.Equal(DisputeStatus.Closed.ToString(), finalDisputeData.Status);
    }
}
