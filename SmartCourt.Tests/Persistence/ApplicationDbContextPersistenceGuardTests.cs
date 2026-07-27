using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Disputes.Entities;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Persistence;

public sealed class ApplicationDbContextPersistenceGuardTests
{
    private static readonly DateTime FixedUtc =
        new(2026, 7, 28, 10, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void DbContext_ExposesEveryContractPaymentDbSet()
    {
        var expectedTypes = new[]
        {
            typeof(Contract),
            typeof(Milestone),
            typeof(MilestoneChangeRequest),
            typeof(MilestoneSubmission),
            typeof(MilestoneSubmissionAttachment),
            typeof(ContractAttachment),
            typeof(EscrowAccount),
            typeof(EscrowHold),
            typeof(EscrowLedgerEntry),
            typeof(PaymentTransaction),
            typeof(LawyerWallet),
            typeof(WithdrawalRequest),
            typeof(Dispute),
            typeof(DisputeResolution),
            typeof(DisputeEvidence),
            typeof(LawyerPenalty),
            typeof(ContractStateHistory),
            typeof(MilestoneStateHistory),
            typeof(IdempotencyRecord),
            typeof(OutboxMessage)
        };

        var dbSetTypes = typeof(ApplicationDbContext)
            .GetProperties()
            .Where(property => property.PropertyType.IsGenericType
                && property.PropertyType.GetGenericTypeDefinition()
                    == typeof(DbSet<>))
            .Select(property => property.PropertyType.GetGenericArguments()[0])
            .ToHashSet();

        Assert.All(expectedTypes, type => Assert.Contains(type, dbSetTypes));
    }

    [Theory]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    public void AppendOnlyRows_RejectModificationAndDeletion(EntityState state)
    {
        var appendOnlyTypes = new[]
        {
            typeof(MilestoneSubmission),
            typeof(MilestoneSubmissionAttachment),
            typeof(EscrowLedgerEntry),
            typeof(DisputeResolution),
            typeof(DisputeEvidence),
            typeof(LawyerPenalty),
            typeof(ContractStateHistory),
            typeof(MilestoneStateHistory)
        };

        foreach (var type in appendOnlyTypes)
        {
            using var context = CreateContext();
            var entity = CreateEntity(type);
            context.Attach(entity);
            context.Entry(entity).State = state;

            var exception = Assert.Throws<BusinessException>(
                context.PrepareTrackedEntriesForSave);

            Assert.Contains("append-only", exception.Message);
        }
    }

    [Theory]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    public async Task SaveChangesAsync_RejectsAppendOnlyMutationBeforeDatabaseCall(
        EntityState state)
    {
        await using var context = CreateContext();
        var entity = CreateEntity(typeof(EscrowLedgerEntry));
        context.Attach(entity);
        context.Entry(entity).State = state;

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => context.SaveChangesAsync());

        Assert.Contains("append-only", exception.Message);
    }

    [Fact]
    public void AddedContractPaymentTimestamp_MustBeUtc()
    {
        using var context = CreateContext();
        var transaction = new PaymentTransaction(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            PaymentOperationType.Deposit,
            "Mock",
            Guid.NewGuid().ToString(),
            100m,
            FixedUtc)
        {
            CreatedAt = DateTime.SpecifyKind(
                FixedUtc,
                DateTimeKind.Local)
        };
        context.PaymentTransactions.Add(transaction);

        var exception = Assert.Throws<BusinessException>(
            context.PrepareTrackedEntriesForSave);

        Assert.Contains("must be UTC", exception.Message);
    }

    [Fact]
    public void ModifiedContractPaymentTimestamp_MustBeUtc()
    {
        using var context = CreateContext();
        var transaction = new PaymentTransaction(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            PaymentOperationType.Deposit,
            "Mock",
            Guid.NewGuid().ToString(),
            100m,
            FixedUtc);
        context.Attach(transaction);
        transaction.UpdatedAt = DateTime.SpecifyKind(
            FixedUtc,
            DateTimeKind.Unspecified);
        context.Entry(transaction)
            .Property(item => item.UpdatedAt)
            .IsModified = true;

        var exception = Assert.Throws<BusinessException>(
            context.PrepareTrackedEntriesForSave);

        Assert.Contains("must be UTC", exception.Message);
    }

    [Fact]
    public void LegacyAuditing_UsesInjectedClockAndCurrentUser()
    {
        var actorId = Guid.NewGuid();
        using var context = CreateContext(
            new FixedTimeProvider(new DateTimeOffset(FixedUtc)),
            new StubCurrentUserService(actorId));
        var document = new LawDocument();
        context.LawDocuments.Add(document);

        context.PrepareTrackedEntriesForSave();

        Assert.Equal(FixedUtc, document.CreatedAt);
        Assert.Equal(actorId.ToString(), document.CreatedBy);
    }

    [Fact]
    public void ContractPaymentEntities_HaveNoSoftDeleteQueryFilters()
    {
        using var context = CreateContext();
        var entityTypes = new[]
        {
            typeof(Contract),
            typeof(Milestone),
            typeof(MilestoneChangeRequest),
            typeof(MilestoneSubmission),
            typeof(MilestoneSubmissionAttachment),
            typeof(ContractAttachment),
            typeof(EscrowAccount),
            typeof(EscrowHold),
            typeof(EscrowLedgerEntry),
            typeof(PaymentTransaction),
            typeof(LawyerWallet),
            typeof(WithdrawalRequest),
            typeof(Dispute),
            typeof(DisputeResolution),
            typeof(DisputeEvidence),
            typeof(LawyerPenalty),
            typeof(ContractStateHistory),
            typeof(MilestoneStateHistory),
            typeof(IdempotencyRecord),
            typeof(OutboxMessage)
        };

        foreach (var type in entityTypes)
        {
            Assert.Null(
                context.Model.FindEntityType(type)!.GetQueryFilter());
        }
    }

    private static object CreateEntity(Type type)
    {
        var entity = Activator.CreateInstance(type, nonPublic: true)!;
        type.GetProperty("Id")!.SetValue(entity, Guid.NewGuid());
        return entity;
    }

    private static ApplicationDbContext CreateContext(
        TimeProvider? timeProvider = null,
        ICurrentUserService? currentUserService = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=SmartCourtGuardTests;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new ApplicationDbContext(
            options,
            timeProvider,
            currentUserService);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }

    private sealed class StubCurrentUserService(Guid userId)
        : ICurrentUserService
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => true;
    }
}
