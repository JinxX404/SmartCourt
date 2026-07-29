using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Cases.Entities;
using SmartCourt.Features.Disputes.Entities;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Interfaces;

namespace SmartCourt.Persistence;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    private static readonly HashSet<Type> AppendOnlyTypes =
    [
        typeof(MilestoneSubmission),
        typeof(MilestoneSubmissionAttachment),
        typeof(EscrowLedgerEntry),
        typeof(PaymentWebhookEvent),
        typeof(DisputeResolution),
        typeof(DisputeEvidence),
        typeof(LawyerPenalty),
        typeof(ContractStateHistory),
        typeof(MilestoneStateHistory)
    ];

    private static readonly HashSet<Type> ContractPaymentTypes =
    [
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
        typeof(PaymentWebhookEvent),
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
    ];

    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService? _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        TimeProvider? timeProvider = null,
        ICurrentUserService? currentUserService = null)
        : base(options)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
        _currentUserService = currentUserService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    public DbSet<UserVerificationDocument> UserVerificationDocuments =>
        Set<UserVerificationDocument>();
    public DbSet<LegalCategory> LegalCategories => Set<LegalCategory>();
    public DbSet<LegalSpecialization> LegalSpecializations =>
        Set<LegalSpecialization>();
    public DbSet<LawDocument> LawDocuments => Set<LawDocument>();
    public DbSet<LegalCase> LegalCases => Set<LegalCase>();
    public DbSet<Proposal> Proposals => Set<Proposal>();

    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<MilestoneChangeRequest> MilestoneChangeRequests =>
        Set<MilestoneChangeRequest>();
    public DbSet<MilestoneSubmission> MilestoneSubmissions =>
        Set<MilestoneSubmission>();
    public DbSet<MilestoneSubmissionAttachment> MilestoneSubmissionAttachments =>
        Set<MilestoneSubmissionAttachment>();
    public DbSet<ContractAttachment> ContractAttachments =>
        Set<ContractAttachment>();
    public DbSet<EscrowAccount> EscrowAccounts => Set<EscrowAccount>();
    public DbSet<EscrowHold> EscrowHolds => Set<EscrowHold>();
    public DbSet<EscrowLedgerEntry> EscrowLedgerEntries =>
        Set<EscrowLedgerEntry>();
    public DbSet<PaymentTransaction> PaymentTransactions =>
        Set<PaymentTransaction>();
    public DbSet<PaymentWebhookEvent> PaymentWebhookEvents =>
        Set<PaymentWebhookEvent>();
    public DbSet<LawyerWallet> LawyerWallets => Set<LawyerWallet>();
    public DbSet<WithdrawalRequest> WithdrawalRequests =>
        Set<WithdrawalRequest>();
    public DbSet<Dispute> Disputes => Set<Dispute>();
    public DbSet<DisputeResolution> DisputeResolutions =>
        Set<DisputeResolution>();
    public DbSet<DisputeEvidence> DisputeEvidence => Set<DisputeEvidence>();
    public DbSet<LawyerPenalty> LawyerPenalties => Set<LawyerPenalty>();
    public DbSet<ContractStateHistory> ContractStateHistories =>
        Set<ContractStateHistory>();
    public DbSet<MilestoneStateHistory> MilestoneStateHistories =>
        Set<MilestoneStateHistory>();
    public DbSet<IdempotencyRecord> IdempotencyRecords =>
        Set<IdempotencyRecord>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PrepareTrackedEntriesForSave();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        PrepareTrackedEntriesForSave();
        return await base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);
    }

    internal void PrepareTrackedEntriesForSave()
    {
        RejectAppendOnlyMutations();
        ApplyLegacyAuditMetadata();
        ValidateContractPaymentTimestamps();
    }

    private void RejectAppendOnlyMutations()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (!AppendOnlyTypes.Contains(entry.Metadata.ClrType)
                || entry.State is not (EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            throw new BusinessException(
                $"{entry.Metadata.ClrType.Name} is append-only and cannot be modified or deleted.");
        }
    }

    private void ApplyLegacyAuditMetadata()
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var actor = _currentUserService?.UserId?.ToString() ?? "System";

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.CreatedBy = actor;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = utcNow;
                    entry.Entity.LastModifiedBy = actor;
                    break;
            }
        }
    }

    private void ValidateContractPaymentTimestamps()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (!ContractPaymentTypes.Contains(entry.Metadata.ClrType)
                || entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            foreach (var property in entry.Properties)
            {
                var propertyType = property.Metadata.ClrType;
                var isTimestamp = propertyType == typeof(DateTime)
                    || propertyType == typeof(DateTime?);
                if (!isTimestamp
                    || entry.State == EntityState.Modified && !property.IsModified
                    || property.CurrentValue is not DateTime value)
                {
                    continue;
                }

                if (value.Kind != DateTimeKind.Utc)
                {
                    throw new BusinessException(
                        $"{entry.Metadata.ClrType.Name}.{property.Metadata.Name} must be UTC.");
                }
            }
        }
    }
}
