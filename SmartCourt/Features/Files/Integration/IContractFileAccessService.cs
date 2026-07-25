namespace SmartCourt.Features.Files.Integration;

public interface IContractFileAccessService
{
    Task<IReadOnlyList<AuthorizedContractFile>> AuthorizeForUseAsync(
        Guid actorUserId,
        IReadOnlyCollection<Guid> storedFileIds,
        ContractFilePurpose purpose,
        Guid relatedEntityId,
        CancellationToken cancellationToken);

    Task<ContractFileReadAccess?> GetAuthorizedReadAccessAsync(
        Guid actorUserId,
        Guid storedFileId,
        ContractFilePurpose purpose,
        Guid relatedEntityId,
        CancellationToken cancellationToken);
}

public sealed record AuthorizedContractFile(
    Guid StoredFileId,
    Guid OwnerUserId);

public sealed record ContractFileReadAccess(
    Guid StoredFileId,
    Uri SignedUri,
    DateTimeOffset ExpiresAt);

public enum ContractFilePurpose
{
    ContractAttachment = 1,
    MilestoneSubmission = 2,
    DisputeEvidence = 3
}
