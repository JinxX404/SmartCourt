using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Cases.Enums;

namespace SmartCourt.Features.Cases.Entities;

public sealed class LegalCase
{
    private LegalCase()
    {
    }

    internal LegalCase(
        Guid id,
        Guid clientUserId,
        string title,
        string description,
        string? caseLocation,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new BusinessException("معرّف القضية مطلوب.");
        }

        if (clientUserId == Guid.Empty)
        {
            throw new BusinessException("معرّف صاحب القضية مطلوب.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new BusinessException("عنوان القضية مطلوب.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new BusinessException("وصف القضية مطلوب.");
        }

        if (createdAt.Kind != DateTimeKind.Utc)
        {
            throw new BusinessException(
                "تاريخ إنشاء القضية يجب أن يكون بالتوقيت العالمي.");
        }

        Id = id;
        ClientUserId = clientUserId;
        Title = title;
        Description = description;
        CaseLocation = caseLocation;
        Status = CaseStatus.Draft;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; internal set; }
    public Guid ClientUserId { get; internal set; }
    public string Title { get; internal set; } = string.Empty;
    public string Description { get; internal set; } = string.Empty;
    public string? CaseLocation { get; internal set; }
    public CaseStatus Status { get; internal set; }
    public DateTime? FinalSubmittedAt { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
}
