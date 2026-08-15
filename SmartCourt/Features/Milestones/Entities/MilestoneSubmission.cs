using SmartCourt.Common.Domain;

namespace SmartCourt.Features.Milestones.Entities;

public sealed class MilestoneSubmission
{
    private MilestoneSubmission()
    {
    }

    internal MilestoneSubmission(
        Guid id,
        Guid milestoneId,
        Guid escrowHoldId,
        Guid submittedByUserId,
        int version,
        string notes,
        DateTimeOffset submittedAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        MilestoneId = EntityGuard.NotEmpty(milestoneId, nameof(milestoneId));
        EscrowHoldId = EntityGuard.NotEmpty(escrowHoldId, nameof(escrowHoldId));
        SubmittedByUserId = EntityGuard.NotEmpty(
            submittedByUserId,
            nameof(submittedByUserId));
        Version = EntityGuard.Positive(version, nameof(version));
        Notes = EntityGuard.Required(notes, nameof(notes));
        SubmittedAt = EntityGuard.Utc(submittedAt, nameof(submittedAt));
    }

    public Guid Id { get; private set; }
    public Guid MilestoneId { get; private set; }
    public Guid EscrowHoldId { get; private set; }
    public Guid SubmittedByUserId { get; private set; }
    public int Version { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public DateTimeOffset SubmittedAt { get; private set; }
}
