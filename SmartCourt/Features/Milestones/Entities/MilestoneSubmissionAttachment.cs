using SmartCourt.Common.Domain;

namespace SmartCourt.Features.Milestones.Entities;

public sealed class MilestoneSubmissionAttachment
{
    private MilestoneSubmissionAttachment()
    {
    }

    internal MilestoneSubmissionAttachment(
        Guid id,
        Guid milestoneSubmissionId,
        Guid storedFileId,
        DateTimeOffset createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        MilestoneSubmissionId = EntityGuard.NotEmpty(
            milestoneSubmissionId,
            nameof(milestoneSubmissionId));
        StoredFileId = EntityGuard.NotEmpty(storedFileId, nameof(storedFileId));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; private set; }
    public Guid MilestoneSubmissionId { get; private set; }
    public Guid StoredFileId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
