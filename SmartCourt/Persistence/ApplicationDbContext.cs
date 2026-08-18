using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Features.Chat.Entities;
using SmartCourt.Features.ChatAgent.Entities;
using SmartCourt.Features.Consultations.Domain.Entities;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Disputes.Entities;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Notifications.Entities;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Ratings.Entities;
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
        typeof(ChatMessage),
        typeof(ChatMessageAttachment),
        typeof(AgentMessage),
        typeof(PaymentWebhookEvent),
        typeof(DisputeResolution),
        typeof(DisputeEvidence),
        typeof(ContractFileAccessAudit),
        typeof(LawyerPenalty),
        typeof(WalletAdjustment),
        typeof(ContractStateHistory),
        typeof(MilestoneStateHistory),
        typeof(ContractRating),
        typeof(ConsultationLedgerEntry),
        typeof(QuotaTransaction)
    ];

    private static readonly HashSet<Type> ContractPaymentTypes =
    [
        typeof(Contract),
        typeof(ContractRating),
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
        typeof(LawyerPayoutAccount),
        typeof(WithdrawalRequest),
        typeof(Dispute),
        typeof(DisputeResolution),
        typeof(DisputeEvidence),
        typeof(ContractFileAccessAudit),
        typeof(LawyerPenalty),
        typeof(WalletAdjustment),
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

        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var properties = entityType.GetProperties().Where(p => p.ClrType == typeof(DateTimeOffset)
                                                                    || p.ClrType == typeof(DateTimeOffset?));
                foreach (var property in properties)
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.DateTimeOffsetToBinaryConverter());
                }

                var rowVersionProperties = entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(byte[]) && p.IsConcurrencyToken);
                foreach (var property in rowVersionProperties)
                {
                    property.SetDefaultValueSql("x'0000000000000001'");
                }
            }
        }
    }

    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    public DbSet<UserVerificationDocument> UserVerificationDocuments =>
        Set<UserVerificationDocument>();
    public DbSet<LegalCategory> LegalCategories => Set<LegalCategory>();
    public DbSet<LegalArticleCategory> LegalArticleCategories => Set<LegalArticleCategory>();
    public DbSet<LegalArticle> LegalArticles => Set<LegalArticle>();
    public DbSet<ArticleView> ArticleViews => Set<ArticleView>();
    public DbSet<ArticleLike> ArticleLikes => Set<ArticleLike>();
    public DbSet<ArticleComment> ArticleComments => Set<ArticleComment>();
    public DbSet<ArticleReport> ArticleReports => Set<ArticleReport>();
    public DbSet<LegalSpecialization> LegalSpecializations =>
        Set<LegalSpecialization>();
    public DbSet<LawDocument> LawDocuments => Set<LawDocument>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<ChatConversation> ChatConversations =>
        Set<ChatConversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatMessageAttachment> ChatMessageAttachments =>
        Set<ChatMessageAttachment>();
    public DbSet<AgentConversation> AgentConversations =>
        Set<AgentConversation>();
    public DbSet<AgentMessage> AgentMessages => Set<AgentMessage>();

    public DbSet<QuotaProfile> QuotaProfiles => Set<QuotaProfile>();
    public DbSet<DailyUsage> DailyUsages => Set<DailyUsage>();
    public DbSet<QuotaLedger> QuotaLedgers => Set<QuotaLedger>();
    public DbSet<QuotaTransaction> QuotaTransactions => Set<QuotaTransaction>();
    public DbSet<TokenUsageHistory> TokenUsageHistories => Set<TokenUsageHistory>();
    
    public DbSet<ModelPricing> ModelPricings => Set<ModelPricing>();
    public DbSet<ModelUsageHistory> ModelUsageHistories => Set<ModelUsageHistory>();
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
    public DbSet<LawyerPayoutAccount> LawyerPayoutAccounts =>
        Set<LawyerPayoutAccount>();
    public DbSet<ClientPaymentCustomer> ClientPaymentCustomers =>
        Set<ClientPaymentCustomer>();
    public DbSet<WithdrawalRequest> WithdrawalRequests =>
        Set<WithdrawalRequest>();
    public DbSet<Dispute> Disputes => Set<Dispute>();
    public DbSet<DisputeResolution> DisputeResolutions =>
        Set<DisputeResolution>();
    public DbSet<DisputeEvidence> DisputeEvidence => Set<DisputeEvidence>();
    public DbSet<ContractFileAccessAudit> ContractFileAccessAudits =>
        Set<ContractFileAccessAudit>();
    public DbSet<LawyerPenalty> LawyerPenalties => Set<LawyerPenalty>();
    public DbSet<WalletAdjustment> WalletAdjustments =>
        Set<WalletAdjustment>();
    public DbSet<ContractStateHistory> ContractStateHistories =>
        Set<ContractStateHistory>();
    public DbSet<MilestoneStateHistory> MilestoneStateHistories =>
        Set<MilestoneStateHistory>();
    public DbSet<IdempotencyRecord> IdempotencyRecords =>
        Set<IdempotencyRecord>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ContractRating> ContractRatings => Set<ContractRating>();
    public DbSet<LawyerConsultationSettings> LawyerConsultationSettings => Set<LawyerConsultationSettings>();
    public DbSet<ConsultationOffering> ConsultationOfferings => Set<ConsultationOffering>();
    public DbSet<ConsultationOfferingInclusion> ConsultationOfferingInclusions => Set<ConsultationOfferingInclusion>();
    public DbSet<ConsultationAvailabilitySlot> ConsultationAvailabilitySlots => Set<ConsultationAvailabilitySlot>();
    public DbSet<ConsultationBooking> ConsultationBookings => Set<ConsultationBooking>();
    public DbSet<ConsultationPaymentTransaction> ConsultationPaymentTransactions => Set<ConsultationPaymentTransaction>();
    public DbSet<ConsultationEscrowHold> ConsultationEscrowHolds => Set<ConsultationEscrowHold>();
    public DbSet<ConsultationLedgerEntry> ConsultationLedgerEntries => Set<ConsultationLedgerEntry>();
    public DbSet<TokenBundlePaymentTransaction> TokenBundlePaymentTransactions => Set<TokenBundlePaymentTransaction>();

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

            // The received Stripe event identity and payload metadata are
            // immutable. Only its processing checkpoint may advance so that
            // Stripe can retry a delivery safely after a transient failure.
            if (entry.Metadata.ClrType == typeof(PaymentWebhookEvent)
                && entry.State == EntityState.Modified
                && entry.Properties
                    .Where(property => property.IsModified)
                    .All(property => property.Metadata.Name is
                        nameof(PaymentWebhookEvent.ProcessedAt)
                        or nameof(PaymentWebhookEvent.ProcessingError)))
            {
                continue;
            }

            throw new BusinessException(
                $"السجل المالي أو التدقيقي من النوع {entry.Metadata.ClrType.Name} للإضافة فقط ولا يمكن تعديله أو حذفه.");
        }
    }
    public DbSet<Case> Cases { get; set; }
    public DbSet<CaseDocument> CaseDocuments { get; set; }
    public DbSet<CaseProfile> CaseProfiles { get; set; }
    public DbSet<CaseReviewReport> CaseReviewReports { get; set; }
    public DbSet<CaseRecommendation> CaseRecommendations { get; set; }
    public DbSet<ReviewPoint> ReviewPoints { get; set; }
    public DbSet<ClientProfile> ClientProfile { get; set; }
    public DbSet<LawyerProfile> lawyerProfile { get; set; }
    public DbSet<LawyerProfile> LawyerProfiles => lawyerProfile;
    public DbSet<LawyerSpecialization> LawyerSpecializations { get; set; }

    private void ApplyLegacyAuditMetadata()
    {
        var utcNow = _timeProvider.GetUtcNow();
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
                var isTimestamp = propertyType == typeof(DateTimeOffset)
                    || propertyType == typeof(DateTimeOffset?);
                if (!isTimestamp
                    || entry.State == EntityState.Modified && !property.IsModified
                    || property.CurrentValue is not DateTimeOffset value)
                {
                    continue;
                }

                if (value.Offset != TimeSpan.Zero)
                {
                    throw new BusinessException(
                        $"يجب أن تكون قيمة {entry.Metadata.ClrType.Name}.{property.Metadata.Name} بالتوقيت العالمي المنسق.");
                }
            }
        }
    }
}
