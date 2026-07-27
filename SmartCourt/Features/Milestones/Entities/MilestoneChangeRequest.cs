using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Milestones.Enums;

namespace SmartCourt.Features.Milestones.Entities;

public sealed class MilestoneChangeRequest
{
    private MilestoneChangeRequest()
    {
    }

    internal MilestoneChangeRequest(
        Guid id,
        Guid milestoneId,
        Guid requestedByUserId,
        string? proposedDescription,
        int? proposedDurationDays,
        DateTime? proposedDueDate,
        string reason,
        DateTime createdAt)
    {
        if (proposedDescription is null
            && proposedDurationDays is null
            && proposedDueDate is null)
        {
            throw new BusinessException(
                "A change request must propose at least one change.");
        }

        Id = EntityGuard.NotEmpty(id, nameof(id));
        MilestoneId = EntityGuard.NotEmpty(milestoneId, nameof(milestoneId));
        RequestedByUserId = EntityGuard.NotEmpty(
            requestedByUserId,
            nameof(requestedByUserId));
        ProposedDescription = proposedDescription;
        if (proposedDurationDays.HasValue)
        {
            EntityGuard.Positive(
                proposedDurationDays.Value,
                nameof(proposedDurationDays));
        }

        ProposedDurationDays = proposedDurationDays;
        ProposedDueDate = EntityGuard.OptionalUtc(
            proposedDueDate,
            nameof(proposedDueDate));
        Reason = EntityGuard.Required(reason, nameof(reason));
        Status = ChangeRequestStatus.Pending;
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; internal set; }
    public Guid MilestoneId { get; internal set; }
    public Guid RequestedByUserId { get; internal set; }
    public string? ProposedDescription { get; internal set; }
    public int? ProposedDurationDays { get; internal set; }
    public DateTime? ProposedDueDate { get; internal set; }
    public string Reason { get; internal set; } = string.Empty;
    public ChangeRequestStatus Status { get; internal set; }
    public Guid? DecidedByUserId { get; internal set; }
    public DateTime? DecidedAt { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
    public DateTime CreatedAt { get; internal set; }
}
