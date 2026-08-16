namespace SmartCourt.Features.Contracts.Files;

public interface IContractFileService
{
    Task<IReadOnlyList<ContractFileDto>> UploadAsync(
        Guid contractId,
        UploadContractFilesRequest request,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Guid contractId,
        Guid storedFileId,
        CancellationToken cancellationToken);
}
