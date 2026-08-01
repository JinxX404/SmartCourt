using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Files;
using SmartCourt.Features.Disputes.Entities;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Files.Integration;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Contracts;

public sealed class ContractScopedFileAccessServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 1, 11, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task MilestoneUse_RequiresOwnerAndContractLawyer()
    {
        await using var context = CreateContext();
        var state = AddContractAndMilestone(context);
        var lawyerFile = AddOwnedFile(context, state.Contract.LawyerUserId);
        var clientFile = AddOwnedFile(context, state.Contract.ClientUserId);
        await context.SaveChangesAsync();
        var service = CreateService(context, new StubEligibilityService());

        var authorized = await service.AuthorizeForUseAsync(
            state.Contract.LawyerUserId,
            [lawyerFile.Id],
            ContractFilePurpose.MilestoneSubmission,
            state.Milestone.Id,
            CancellationToken.None);

        Assert.Equal(lawyerFile.Id, Assert.Single(authorized).StoredFileId);
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.AuthorizeForUseAsync(
                state.Contract.ClientUserId,
                [clientFile.Id],
                ContractFilePurpose.MilestoneSubmission,
                state.Milestone.Id,
                CancellationToken.None));
    }

    [Fact]
    public async Task Use_RejectsCrossContractRelatedEntitySubstitution()
    {
        await using var context = CreateContext();
        var first = AddContractAndMilestone(context);
        var second = AddContractAndMilestone(context);
        var file = AddOwnedFile(context, first.Contract.LawyerUserId);
        await context.SaveChangesAsync();
        var service = CreateService(context, new StubEligibilityService());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.AuthorizeForUseAsync(
                first.Contract.LawyerUserId,
                [file.Id],
                ContractFilePurpose.MilestoneSubmission,
                second.Milestone.Id,
                CancellationToken.None));
    }

    [Fact]
    public async Task Read_RequiresRecordedAttachmentAndAuditsModeratorAccess()
    {
        await using var context = CreateContext();
        var state = AddContractAndMilestone(context);
        var file = AddOwnedFile(context, state.Contract.LawyerUserId);
        var submission = new MilestoneSubmission(
            Guid.NewGuid(),
            state.Milestone.Id,
            Guid.NewGuid(),
            state.Contract.LawyerUserId,
            1,
            "تم تسليم مستند المرحلة.",
            Now.UtcDateTime);
        context.AddRange(
            submission,
            new MilestoneSubmissionAttachment(
                Guid.NewGuid(),
                submission.Id,
                file.Id,
                Now.UtcDateTime));
        await context.SaveChangesAsync();
        var moderatorUserId = Guid.NewGuid();
        var eligibility = new StubEligibilityService();
        eligibility.Results[moderatorUserId] = new ContractUserEligibilityFacts(
            moderatorUserId,
            IsActive: true,
            CanActAsClient: false,
            CanActAsLawyer: false,
            CanActAsModerator: true,
            CanActAsFinanceAdministrator: false,
            CanActAsSuperAdministrator: false);
        var service = CreateService(context, eligibility);

        var access = await service.GetAuthorizedReadAccessAsync(
            moderatorUserId,
            file.Id,
            ContractFilePurpose.MilestoneSubmission,
            state.Milestone.Id,
            CancellationToken.None);

        Assert.NotNull(access);
        Assert.Equal(
            new Uri("https://files.example/contracts/file.pdf"),
            access.SignedUri);
        var audit = await context.ContractFileAccessAudits.SingleAsync();
        Assert.Equal(moderatorUserId, audit.ActorUserId);
        Assert.True(audit.ModeratorAccess);
        Assert.Equal(state.Milestone.Id, audit.RelatedEntityId);

        var unrelatedFile = AddOwnedFile(
            context,
            state.Contract.LawyerUserId);
        await context.SaveChangesAsync();
        var missing = await service.GetAuthorizedReadAccessAsync(
            state.Contract.ClientUserId,
            unrelatedFile.Id,
            ContractFilePurpose.MilestoneSubmission,
            state.Milestone.Id,
            CancellationToken.None);
        Assert.Null(missing);
    }

    [Fact]
    public async Task DisputeEvidenceUse_AcceptsTrackedNewDisputeForParticipantOnly()
    {
        await using var context = CreateContext();
        var state = AddContractAndMilestone(context);
        var file = AddOwnedFile(context, state.Contract.ClientUserId);
        await context.SaveChangesAsync();
        var dispute = new Dispute(
            Guid.NewGuid(),
            state.Contract.Id,
            state.Milestone.Id,
            state.Contract.ClientUserId,
            DisputeCategory.Payment,
            "نزاع مالي",
            "وصف واضح للنزاع المالي المتعلق بالمرحلة.",
            DisputeRequestedOutcome.Refund,
            Now.UtcDateTime);
        context.Disputes.Add(dispute);
        var service = CreateService(context, new StubEligibilityService());

        var authorized = await service.AuthorizeForUseAsync(
            state.Contract.ClientUserId,
            [file.Id],
            ContractFilePurpose.DisputeEvidence,
            dispute.Id,
            CancellationToken.None);

        Assert.Equal(file.Id, Assert.Single(authorized).StoredFileId);
    }

    private static ContractScopedFileAccessService CreateService(
        ApplicationDbContext context,
        IContractUserEligibilityService eligibilityService)
    {
        return new ContractScopedFileAccessService(
            context,
            new TestFileStorageService(),
            eligibilityService,
            new FixedTimeProvider());
    }

    private static TestState AddContractAndMilestone(
        ApplicationDbContext context)
    {
        var contract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "عقد اختبار الملفات",
            "شروط صالحة لاختبار صلاحيات الملفات.",
            Now.UtcDateTime);
        var milestone = new Milestone(
            Guid.NewGuid(),
            contract.Id,
            "مرحلة اختبار الملفات",
            null,
            1,
            1_000m,
            14,
            null,
            Now.UtcDateTime);
        context.AddRange(contract, milestone);
        return new TestState(contract, milestone);
    }

    private static StoredFile AddOwnedFile(
        ApplicationDbContext context,
        Guid ownerUserId)
    {
        var file = new StoredFile
        {
            Id = Guid.NewGuid(),
            StoredFileName = "file.pdf",
            OriginalFileName = "file.pdf",
            FileUrl = "contracts/file.pdf",
            ContentType = "application/pdf",
            Extension = ".pdf",
            SizeInBytes = 100
        };
        context.StoredFiles.Add(file);
        context.UserVerificationDocuments.Add(
            new UserVerificationDocument
            {
                Id = Guid.NewGuid(),
                UserId = ownerUserId,
                StoredFileId = file.Id,
                StoredFile = file,
                DocumentType = VerificationDocumentType.NationalIdFront,
                Status = VerificationDocumentStatus.Verified,
                ExpirationDate = new DateOnly(2030, 1, 1),
                IsCurrent = true
            });
        return file;
    }

    private static ApplicationDbContext CreateContext()
    {
        return new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(
                    $"contract-file-scope-{Guid.NewGuid():N}")
                .Options,
            new FixedTimeProvider());
    }

    private sealed record TestState(
        Contract Contract,
        Milestone Milestone);

    private sealed class StubEligibilityService
        : IContractUserEligibilityService
    {
        public Dictionary<Guid, ContractUserEligibilityFacts> Results { get; }
            = [];

        public Task<ContractUserEligibilityFacts?> FindEligibilityAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Results.TryGetValue(userId, out var facts);
            return Task.FromResult(facts);
        }
    }

    private sealed class TestFileStorageService : IFileStorageService
    {
        public Task<string> GetDownloadUrlAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult($"https://files.example/{filePath}");
        }

        public Task<FileUploadResult> UploadAsync(
            Stream stream,
            string filePath,
            string originalFileName,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<byte[]> DownloadAsync(
            string filePath,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task DeleteAsync(
            string filePath,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ExistsAsync(
            string filePath,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
