using SmartCourt.Common.Domain;
using SmartCourt.Features.Disputes.Enums;

namespace SmartCourt.Features.Disputes.Entities;

public sealed class Dispute
{
    private Dispute()
    {
    }

    internal Dispute(
        Guid id,
        Guid contractId,
        Guid milestoneId,
        Guid raisedByUserId,
        DisputeCategory category,
        string title,
        string description,
        DisputeRequestedOutcome requestedOutcome,
        DateTimeOffset createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ContractId = EntityGuard.NotEmpty(contractId, nameof(contractId));
        MilestoneId = EntityGuard.NotEmpty(milestoneId, nameof(milestoneId));
        RaisedByUserId = EntityGuard.NotEmpty(
            raisedByUserId,
            nameof(raisedByUserId));
        Category = category;
        Title = EntityGuard.Required(title, nameof(title));
        Description = EntityGuard.Required(description, nameof(description));
        Status = DisputeStatus.Open;
        RequestedOutcome = requestedOutcome;
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; internal set; }
    public Guid ContractId { get; internal set; }
    public Guid MilestoneId { get; internal set; }
    public Guid RaisedByUserId { get; internal set; }
    public Guid? AssignedModeratorUserId { get; internal set; }
    public DisputeCategory Category { get; internal set; }
    public string Title { get; internal set; } = string.Empty;
    public string Description { get; internal set; } = string.Empty;
    public DisputeStatus Status { get; internal set; }
    public DisputeRequestedOutcome RequestedOutcome { get; internal set; }
    public DisputeResolutionType? ResolutionType { get; internal set; }
    public decimal? ResolutionAmount { get; internal set; }
    public string? ResolutionSummary { get; internal set; }
    public Guid? ResolvedByUserId { get; internal set; }
    public DateTimeOffset? ResolvedAt { get; internal set; }
    public DateTimeOffset? ClosedAt { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
    public DateTimeOffset CreatedAt { get; internal set; }
    public DateTimeOffset UpdatedAt { get; internal set; }
}
