using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Ratings.Enums;

namespace SmartCourt.Features.Ratings.Entities;

public sealed class ContractRating
{
    private ContractRating()
    {
    }

    internal ContractRating(
        Guid id,
        Guid contractId,
        Guid raterUserId,
        Guid ratedUserId,
        RaterRole raterRole,
        int stars,
        string? comment,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ContractId = EntityGuard.NotEmpty(contractId, nameof(contractId));
        RaterUserId = EntityGuard.NotEmpty(raterUserId, nameof(raterUserId));
        RatedUserId = EntityGuard.NotEmpty(ratedUserId, nameof(ratedUserId));

        if (!Enum.IsDefined(typeof(RaterRole), raterRole))
        {
            throw new BusinessException("دور المقيّم غير صالح.");
        }

        if (stars is < 1 or > 5)
        {
            throw new BusinessException("يجب أن يكون التقييم بين 1 و 5 نجوم.");
        }

        if (comment is not null && comment.Length > 500)
        {
            throw new BusinessException("يجب ألا يتجاوز التعليق 500 حرف.");
        }

        if (raterUserId == ratedUserId)
        {
            throw new BusinessException("لا يمكن للمستخدم تقييم نفسه.");
        }

        RaterRole = raterRole;
        Stars = stars;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; internal set; }
    public Guid ContractId { get; internal set; }
    public Guid RaterUserId { get; internal set; }
    public Guid RatedUserId { get; internal set; }
    public RaterRole RaterRole { get; internal set; }
    public int Stars { get; internal set; }
    public string? Comment { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
}
