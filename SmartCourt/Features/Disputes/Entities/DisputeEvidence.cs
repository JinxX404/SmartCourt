using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;

namespace SmartCourt.Features.Disputes.Entities;

public sealed class DisputeEvidence
{
    private DisputeEvidence()
    {
    }

    internal DisputeEvidence(
        Guid id,
        Guid disputeId,
        Guid uploadedByUserId,
        Guid? storedFileId,
        string? content,
        DateTime createdAt)
    {
        if (!storedFileId.HasValue && string.IsNullOrWhiteSpace(content))
        {
            throw new BusinessException(
                "Dispute evidence requires a stored file or content.");
        }

        Id = EntityGuard.NotEmpty(id, nameof(id));
        DisputeId = EntityGuard.NotEmpty(disputeId, nameof(disputeId));
        UploadedByUserId = EntityGuard.NotEmpty(
            uploadedByUserId,
            nameof(uploadedByUserId));
        StoredFileId = EntityGuard.OptionalGuid(
            storedFileId,
            nameof(storedFileId));
        Content = content;
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; private set; }
    public Guid DisputeId { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public Guid? StoredFileId { get; private set; }
    public string? Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
