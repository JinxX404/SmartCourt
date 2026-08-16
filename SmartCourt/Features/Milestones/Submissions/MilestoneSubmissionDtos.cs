namespace SmartCourt.Features.Milestones.DTOs;

public sealed record MilestoneSubmissionAttachmentDto(
    Guid StoredFileId,
    string FileName,
    string ContentType,
    long SizeInBytes);

public sealed record MilestoneSubmissionDto(
    Guid Id,
    Guid MilestoneId,
    Guid EscrowHoldId,
    Guid SubmittedByUserId,
    int Version,
    string Notes,
    DateTimeOffset SubmittedAt,
    bool IsCurrent,
    IReadOnlyList<MilestoneSubmissionAttachmentDto> Attachments);

public sealed record MilestoneSubmissionFileAccessDto(
    Guid StoredFileId,
    string Url,
    DateTimeOffset ExpiresAt);
