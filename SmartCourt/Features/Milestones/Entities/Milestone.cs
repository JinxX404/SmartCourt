using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Milestones.Enums;

namespace SmartCourt.Features.Milestones.Entities;

public sealed class Milestone
{
    private Milestone()
    {
    }

    internal Milestone(
        Guid id,
        Guid contractId,
        string title,
        string? description,
        int orderNumber,
        decimal amount,
        int? durationDays,
        DateTimeOffset? dueDate,
        DateTimeOffset createdAt)
        : this(
            id,
            contractId,
            title,
            description,
            orderNumber,
            amount,
            durationDays,
            dueDate,
            deliverables: null,
            createdAt)
    {
    }

    internal Milestone(
        Guid id,
        Guid contractId,
        string title,
        string? description,
        int orderNumber,
        decimal amount,
        int? durationDays,
        DateTimeOffset? dueDate,
        IReadOnlyList<string>? deliverables,
        DateTimeOffset createdAt,
        MilestoneType type = MilestoneType.Standard)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ContractId = EntityGuard.NotEmpty(contractId, nameof(contractId));
        Title = EntityGuard.Required(title, nameof(title));
        Description = description;
        OrderNumber = EntityGuard.Positive(orderNumber, nameof(orderNumber));
        Amount = EntityGuard.PositiveMoney(amount, nameof(amount));
        if (durationDays.HasValue)
        {
            EntityGuard.Positive(durationDays.Value, nameof(durationDays));
        }

        if (!Enum.IsDefined(type))
        {
            throw new BusinessException("نوع المرحلة غير صالح.");
        }

        if (type == MilestoneType.Expense
            && (durationDays.HasValue || deliverables is not null))
        {
            throw new BusinessException(
                "مرحلة المصروفات لا تقبل مدة أو مخرجات عمل.");
        }

        Type = type;
        DurationDays = durationDays;
        DueDate = EntityGuard.OptionalUtc(dueDate, nameof(dueDate));
        Deliverables = deliverables?.ToList();
        Status = MilestoneStatus.Draft;
        SubmissionVersion = 0;
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; internal set; }
    public Guid ContractId { get; internal set; }
    public string Title { get; internal set; } = string.Empty;
    public string? Description { get; internal set; }
    public List<string>? Deliverables { get; internal set; }
    public MilestoneType Type { get; internal set; }
    public int OrderNumber { get; internal set; }
    public decimal Amount { get; internal set; }
    public int? DurationDays { get; internal set; }
    public DateTimeOffset? DueDate { get; internal set; }
    public MilestoneStatus Status { get; internal set; }
    public DateTimeOffset? AcceptedByClientAt { get; internal set; }
    public DateTimeOffset? AcceptedByLawyerAt { get; internal set; }
    public DateTimeOffset? ReadyForFundingAt { get; internal set; }
    public DateTimeOffset? FundedAt { get; internal set; }
    public DateTimeOffset? SubmittedAt { get; internal set; }
    public DateTimeOffset? AutoAcceptEligibleAt { get; internal set; }
    public string? AutoAcceptJobId { get; internal set; }
    public DateTimeOffset? AcceptedAt { get; internal set; }
    public MilestoneAcceptanceSource? AcceptanceSource { get; internal set; }
    public DateTimeOffset? HoldStartsAt { get; internal set; }
    public DateTimeOffset? HoldExpiresAt { get; internal set; }
    public DateTimeOffset? ReleasedAt { get; internal set; }
    public DateTimeOffset? RefundedAt { get; internal set; }
    public string? RejectionReason { get; internal set; }
    public int SubmissionVersion { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
    public DateTimeOffset CreatedAt { get; internal set; }
    public DateTimeOffset UpdatedAt { get; internal set; }
}
