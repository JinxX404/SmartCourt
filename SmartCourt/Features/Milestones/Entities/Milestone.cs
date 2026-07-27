using SmartCourt.Common.Domain;
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
        DateTime? dueDate,
        DateTime createdAt)
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

        DurationDays = durationDays;
        DueDate = EntityGuard.OptionalUtc(dueDate, nameof(dueDate));
        Status = MilestoneStatus.Draft;
        SubmissionVersion = 0;
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; internal set; }
    public Guid ContractId { get; internal set; }
    public string Title { get; internal set; } = string.Empty;
    public string? Description { get; internal set; }
    public int OrderNumber { get; internal set; }
    public decimal Amount { get; internal set; }
    public int? DurationDays { get; internal set; }
    public DateTime? DueDate { get; internal set; }
    public MilestoneStatus Status { get; internal set; }
    public DateTime? AcceptedByClientAt { get; internal set; }
    public DateTime? AcceptedByLawyerAt { get; internal set; }
    public DateTime? ReadyForFundingAt { get; internal set; }
    public DateTime? FundedAt { get; internal set; }
    public DateTime? SubmittedAt { get; internal set; }
    public DateTime? AutoAcceptEligibleAt { get; internal set; }
    public string? AutoAcceptJobId { get; internal set; }
    public DateTime? AcceptedAt { get; internal set; }
    public MilestoneAcceptanceSource? AcceptanceSource { get; internal set; }
    public DateTime? HoldStartsAt { get; internal set; }
    public DateTime? HoldExpiresAt { get; internal set; }
    public DateTime? ReleasedAt { get; internal set; }
    public DateTime? RefundedAt { get; internal set; }
    public string? RejectionReason { get; internal set; }
    public int SubmissionVersion { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
}
