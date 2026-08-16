using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Files.Integration;
using SmartCourt.Features.Milestones;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Milestones;

public sealed class MilestoneSubmissionQueryServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task List_ReturnsNewestFirstWithCurrentVersionAndSafeAttachments()
    {
        await using var context = CreateContext();
        var state = AddState(context);
        var first = AddSubmission(context, state, 1, "النسخة الأولى");
        var second = AddSubmission(context, state, 2, "النسخة الثانية");
        state.Milestone.SubmissionVersion = 2;
        AddAttachment(context, first.Id, "first.pdf");
        AddAttachment(context, second.Id, "second.pdf");
        await context.SaveChangesAsync();
        var service = CreateService(context, state.Contract.ClientUserId);

        var result = await service.ListAsync(
            state.Milestone.Id,
            CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(second.Id, result[0].Id);
        Assert.True(result[0].IsCurrent);
        Assert.False(result[1].IsCurrent);
        Assert.Equal("second.pdf", Assert.Single(result[0].Attachments).FileName);
    }

    [Fact]
    public async Task List_RejectsUnrelatedUser()
    {
        await using var context = CreateContext();
        var state = AddState(context);
        await context.SaveChangesAsync();
        var service = CreateService(context, Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.ListAsync(state.Milestone.Id, CancellationToken.None));
    }

    [Fact]
    public async Task FileAccess_RequiresExactSubmissionRelationship()
    {
        await using var context = CreateContext();
        var state = AddState(context);
        var submission = AddSubmission(context, state, 1, "تسليم");
        var file = AddAttachment(context, submission.Id, "deliverable.pdf");
        await context.SaveChangesAsync();
        var fileAccess = new StubFileAccessService
        {
            ReadAccess = new ContractFileReadAccess(
                file.Id,
                new Uri("https://files.example/deliverable.pdf"),
                Now.AddMinutes(5))
        };
        var service = CreateService(
            context,
            state.Contract.ClientUserId,
            fileAccess);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetFileAccessAsync(
                state.Milestone.Id,
                Guid.NewGuid(),
                file.Id,
                CancellationToken.None));
        Assert.Equal(0, fileAccess.ReadCallCount);

        var result = await service.GetFileAccessAsync(
            state.Milestone.Id,
            submission.Id,
            file.Id,
            CancellationToken.None);
        Assert.Equal("https://files.example/deliverable.pdf", result.Url);
        Assert.Equal(1, fileAccess.ReadCallCount);
    }

    private static MilestoneSubmissionQueryService CreateService(
        ApplicationDbContext context,
        Guid actorUserId,
        StubFileAccessService? fileAccess = null)
        => new(
            context,
            new StubCurrentUserService(actorUserId),
            new StubEligibilityService(),
            fileAccess ?? new StubFileAccessService());

    private static TestState AddState(ApplicationDbContext context)
    {
        var contract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "عقد تسليم",
            "شروط عقد لاختبار تسليم المرحلة.",
            Now);
        var milestone = new Milestone(
            Guid.NewGuid(),
            contract.Id,
            "مرحلة تسليم",
            null,
            1,
            1_000m,
            10,
            null,
            Now);
        context.AddRange(contract, milestone);
        return new TestState(contract, milestone);
    }

    private static MilestoneSubmission AddSubmission(
        ApplicationDbContext context,
        TestState state,
        int version,
        string notes)
    {
        var submission = new MilestoneSubmission(
            Guid.NewGuid(),
            state.Milestone.Id,
            Guid.NewGuid(),
            state.Contract.LawyerUserId,
            version,
            notes,
            Now.AddMinutes(version));
        context.MilestoneSubmissions.Add(submission);
        return submission;
    }

    private static StoredFile AddAttachment(
        ApplicationDbContext context,
        Guid submissionId,
        string fileName)
    {
        var file = new StoredFile
        {
            Id = Guid.NewGuid(),
            StoredFileName = fileName,
            OriginalFileName = fileName,
            FileUrl = $"contracts/{fileName}",
            ContentType = "application/pdf",
            Extension = ".pdf",
            SizeInBytes = 128
        };
        context.StoredFiles.Add(file);
        context.MilestoneSubmissionAttachments.Add(
            new MilestoneSubmissionAttachment(
                Guid.NewGuid(),
                submissionId,
                file.Id,
                Now));
        return file;
    }

    private static ApplicationDbContext CreateContext()
        => new(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"submission-query-{Guid.NewGuid():N}")
                .Options,
            new FixedTimeProvider());

    private sealed record TestState(Contract Contract, Milestone Milestone);

    private sealed class StubCurrentUserService(Guid userId)
        : ICurrentUserService
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => true;
    }

    private sealed class StubEligibilityService
        : IContractUserEligibilityService
    {
        public Task<ContractUserEligibilityFacts?> FindEligibilityAsync(
            Guid userId,
            CancellationToken cancellationToken)
            => Task.FromResult<ContractUserEligibilityFacts?>(null);
    }

    private sealed class StubFileAccessService : IContractFileAccessService
    {
        public ContractFileReadAccess? ReadAccess { get; init; }
        public int ReadCallCount { get; private set; }

        public Task<IReadOnlyList<AuthorizedContractFile>> AuthorizeForUseAsync(
            Guid actorUserId,
            IReadOnlyCollection<Guid> storedFileIds,
            ContractFilePurpose purpose,
            Guid relatedEntityId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ContractFileReadAccess?> GetAuthorizedReadAccessAsync(
            Guid actorUserId,
            Guid storedFileId,
            ContractFilePurpose purpose,
            Guid relatedEntityId,
            CancellationToken cancellationToken)
        {
            ReadCallCount++;
            return Task.FromResult(ReadAccess);
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
