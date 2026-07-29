using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Proposals.Enums;

namespace SmartCourt.Features.Proposals.Entities;

public sealed class Proposal
{
    private Proposal()
    {
    }

    internal Proposal(
        Guid id,
        Guid legalCaseId,
        Guid clientUserId,
        Guid lawyerUserId,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new BusinessException("معرّف العرض مطلوب.");
        }

        if (legalCaseId == Guid.Empty)
        {
            throw new BusinessException("معرّف قضية العرض مطلوب.");
        }

        if (clientUserId == Guid.Empty)
        {
            throw new BusinessException("معرّف عميل العرض مطلوب.");
        }

        if (lawyerUserId == Guid.Empty)
        {
            throw new BusinessException("معرّف محامي العرض مطلوب.");
        }

        if (createdAt.Kind != DateTimeKind.Utc)
        {
            throw new BusinessException(
                "تاريخ إنشاء العرض يجب أن يكون بالتوقيت العالمي.");
        }

        Id = id;
        LegalCaseId = legalCaseId;
        ClientUserId = clientUserId;
        LawyerUserId = lawyerUserId;
        Status = ProposalStatus.Pending;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; internal set; }
    public Guid LegalCaseId { get; internal set; }
    public Guid ClientUserId { get; internal set; }
    public Guid LawyerUserId { get; internal set; }
    public ProposalStatus Status { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
}
