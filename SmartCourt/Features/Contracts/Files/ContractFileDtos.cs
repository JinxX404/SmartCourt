namespace SmartCourt.Features.Contracts.Files;

public sealed class UploadContractFilesRequest
{
    public IReadOnlyList<IFormFile> Files { get; init; } = [];
}

public sealed record ContractFileDto(
    Guid StoredFileId,
    string FileName,
    string ContentType,
    long SizeInBytes,
    DateTimeOffset UploadedAt);
