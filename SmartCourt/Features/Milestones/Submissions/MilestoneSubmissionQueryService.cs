using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Files.Integration;
using SmartCourt.Features.Milestones.DTOs;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Milestones;

public sealed class MilestoneSubmissionQueryService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IContractUserEligibilityService userEligibilityService,
    IContractFileAccessService fileAccessService)
    : IMilestoneSubmissionQueryService
{
    public async Task<IReadOnlyList<MilestoneSubmissionDto>> ListAsync(
        Guid milestoneId,
        CancellationToken cancellationToken)
    {
        var access = await EnsureReadAccessAsync(
            milestoneId,
            cancellationToken);
        var submissions = await dbContext.MilestoneSubmissions
            .AsNoTracking()
            .Where(item => item.MilestoneId == milestoneId)
            .OrderByDescending(item => item.Version)
            .Select(item => new
            {
                item.Id,
                item.MilestoneId,
                item.EscrowHoldId,
                item.SubmittedByUserId,
                item.Version,
                item.Notes,
                item.SubmittedAt
            })
            .ToListAsync(cancellationToken);
        if (submissions.Count == 0)
        {
            return [];
        }

        var submissionIds = submissions.Select(item => item.Id).ToArray();
        var attachments = await (
            from attachment in dbContext.MilestoneSubmissionAttachments
                .AsNoTracking()
            join file in dbContext.StoredFiles.AsNoTracking()
                on attachment.StoredFileId equals file.Id
            where submissionIds.Contains(attachment.MilestoneSubmissionId)
                && !file.IsDeleted
            orderby attachment.CreatedAt
            select new
            {
                attachment.MilestoneSubmissionId,
                Attachment = new MilestoneSubmissionAttachmentDto(
                    file.Id,
                    file.OriginalFileName,
                    file.ContentType,
                    file.SizeInBytes)
            })
            .ToListAsync(cancellationToken);
        var attachmentsBySubmission = attachments
            .GroupBy(item => item.MilestoneSubmissionId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<MilestoneSubmissionAttachmentDto>)
                    group.Select(item => item.Attachment).ToArray());

        return submissions.Select(item => new MilestoneSubmissionDto(
                item.Id,
                item.MilestoneId,
                item.EscrowHoldId,
                item.SubmittedByUserId,
                item.Version,
                item.Notes,
                item.SubmittedAt,
                item.Version == access.SubmissionVersion,
                attachmentsBySubmission.GetValueOrDefault(item.Id, [])))
            .ToArray();
    }

    public async Task<MilestoneSubmissionFileAccessDto> GetFileAccessAsync(
        Guid milestoneId,
        Guid submissionId,
        Guid storedFileId,
        CancellationToken cancellationToken)
    {
        await EnsureReadAccessAsync(milestoneId, cancellationToken);
        var attached = await (
            from attachment in dbContext.MilestoneSubmissionAttachments
                .AsNoTracking()
            join submission in dbContext.MilestoneSubmissions.AsNoTracking()
                on attachment.MilestoneSubmissionId equals submission.Id
            where submission.Id == submissionId
                && submission.MilestoneId == milestoneId
                && attachment.StoredFileId == storedFileId
            select attachment.Id)
            .AnyAsync(cancellationToken);
        if (!attached)
        {
            throw new NotFoundException(
                "الملف غير مرتبط بتسليم المرحلة المحدد.");
        }

        var access = await fileAccessService.GetAuthorizedReadAccessAsync(
            GetActorUserId(),
            storedFileId,
            ContractFilePurpose.MilestoneSubmission,
            milestoneId,
            cancellationToken);
        if (access is null)
        {
            throw new NotFoundException("ملف التسليم المطلوب غير موجود.");
        }

        return new MilestoneSubmissionFileAccessDto(
            access.StoredFileId,
            access.SignedUri.AbsoluteUri,
            access.ExpiresAt);
    }

    private async Task<MilestoneAccessFacts> EnsureReadAccessAsync(
        Guid milestoneId,
        CancellationToken cancellationToken)
    {
        if (milestoneId == Guid.Empty)
        {
            throw new BusinessException("معرّف المرحلة مطلوب.");
        }

        var facts = await (
            from milestone in dbContext.Milestones.AsNoTracking()
            join contract in dbContext.Contracts.AsNoTracking()
                on milestone.ContractId equals contract.Id
            where milestone.Id == milestoneId
            select new MilestoneAccessFacts(
                contract.ClientUserId,
                contract.LawyerUserId,
                milestone.SubmissionVersion))
            .SingleOrDefaultAsync(cancellationToken);
        if (facts is null)
        {
            throw new NotFoundException("المرحلة المطلوبة غير موجودة.");
        }

        var actorUserId = GetActorUserId();
        if (actorUserId == facts.ClientUserId
            || actorUserId == facts.LawyerUserId)
        {
            return facts;
        }

        var eligibility = await userEligibilityService.FindEligibilityAsync(
            actorUserId,
            cancellationToken);
        if (eligibility is null
            || !eligibility.IsActive
            || !eligibility.CanActAsModerator
                && !eligibility.CanActAsSuperAdministrator)
        {
            throw new ForbiddenAccessException(
                "لا يملك المستخدم الحالي صلاحية قراءة تسليمات هذه المرحلة.");
        }

        return facts;
    }

    private Guid GetActorUserId()
    {
        if (!currentUserService.IsAuthenticated
            || !currentUserService.UserId.HasValue
            || currentUserService.UserId.Value == Guid.Empty)
        {
            throw new AuthenticationException(
                "يجب تسجيل الدخول لإتمام هذا الإجراء.");
        }

        return currentUserService.UserId.Value;
    }

    private sealed record MilestoneAccessFacts(
        Guid ClientUserId,
        Guid LawyerUserId,
        int SubmissionVersion);
}
