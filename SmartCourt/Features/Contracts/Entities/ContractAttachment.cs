using SmartCourt.Common.Domain;

namespace SmartCourt.Features.Contracts.Entities;

public sealed class ContractAttachment
{
    private ContractAttachment()
    {
    }

    internal ContractAttachment(
        Guid id,
        Guid contractId,
        Guid storedFileId,
        Guid uploadedByUserId,
        DateTimeOffset createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ContractId = EntityGuard.NotEmpty(contractId, nameof(contractId));
        StoredFileId = EntityGuard.NotEmpty(storedFileId, nameof(storedFileId));
        UploadedByUserId = EntityGuard.NotEmpty(
            uploadedByUserId,
            nameof(uploadedByUserId));
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid ContractId { get; private set; }
    public Guid StoredFileId { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
