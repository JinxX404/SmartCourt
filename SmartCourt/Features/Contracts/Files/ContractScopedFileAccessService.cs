using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Files.Integration;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Contracts.Files;

public sealed class ContractScopedFileAccessService(
    ApplicationDbContext dbContext,
    IFileStorageService fileStorageService,
    IContractUserEligibilityService userEligibilityService,
    TimeProvider timeProvider) : IContractFileAccessService
{
    public async Task<IReadOnlyList<AuthorizedContractFile>>
        AuthorizeForUseAsync(
            Guid actorUserId,
            IReadOnlyCollection<Guid> storedFileIds,
            ContractFilePurpose purpose,
            Guid relatedEntityId,
            CancellationToken cancellationToken)
    {
        ValidateRequest(
            actorUserId,
            storedFileIds,
            purpose,
            relatedEntityId);
        var access = await EnsureWriteAccessAsync(
            actorUserId,
            purpose,
            relatedEntityId,
            cancellationToken);

        var requestedIds = storedFileIds.Distinct().ToArray();
        var ownedIds = await (
            from attachment in dbContext.ContractAttachments.AsNoTracking()
            join file in dbContext.StoredFiles.AsNoTracking()
                on attachment.StoredFileId equals file.Id
            where attachment.ContractId == access.ContractId
                && attachment.UploadedByUserId == actorUserId
                && requestedIds.Contains(attachment.StoredFileId)
                && !file.IsDeleted
            select attachment.StoredFileId)
            .Distinct()
            .ToListAsync(cancellationToken);
        if (ownedIds.Count != requestedIds.Length)
        {
            throw new ForbiddenAccessException(
                "أحد الملفات المحددة غير موجود أو لا يملكه المستخدم الحالي، لذلك لا يمكن إرفاقه.");
        }

        return ownedIds
            .Select(fileId => new AuthorizedContractFile(
                fileId,
                actorUserId))
            .ToArray();
    }

    public async Task<ContractFileReadAccess?>
        GetAuthorizedReadAccessAsync(
            Guid actorUserId,
            Guid storedFileId,
            ContractFilePurpose purpose,
            Guid relatedEntityId,
            CancellationToken cancellationToken)
    {
        ValidateRequest(
            actorUserId,
            [storedFileId],
            purpose,
            relatedEntityId);
        var facts = await FindReadFactsAsync(
            storedFileId,
            purpose,
            relatedEntityId,
            cancellationToken);
        if (facts is null)
        {
            return null;
        }

        var participant = actorUserId == facts.ClientUserId
            || actorUserId == facts.LawyerUserId;
        var moderatorAccess = false;
        if (!participant)
        {
            var eligibility = await userEligibilityService
                .FindEligibilityAsync(actorUserId, cancellationToken);
            moderatorAccess = eligibility is not null
                && eligibility.IsActive
                && (eligibility.CanActAsModerator
                    || eligibility.CanActAsSuperAdministrator);
            if (!moderatorAccess)
            {
                throw new ForbiddenAccessException(
                    "لا يملك المستخدم الحالي صلاحية قراءة هذا الملف المرتبط بالعقد.");
            }
        }

        var url = await fileStorageService.GetDownloadUrlAsync(
            facts.StoragePath,
            cancellationToken);
        if (!Uri.TryCreate(url, UriKind.Absolute, out var signedUri))
        {
            throw new BusinessException(
                "تعذر إنشاء رابط آمن لقراءة الملف المطلوب.");
        }

        var now = timeProvider.GetUtcNow();
        dbContext.Set<ContractFileAccessAudit>().Add(
            new ContractFileAccessAudit(
                Guid.NewGuid(),
                actorUserId,
                storedFileId,
                purpose,
                relatedEntityId,
                "قراءة الملف",
                moderatorAccess,
                now));
        await dbContext.SaveChangesAsync(cancellationToken);
        return new ContractFileReadAccess(
            storedFileId,
            signedUri,
            now.AddMinutes(5));
    }

    private async Task<WriteAccessFacts> EnsureWriteAccessAsync(
        Guid actorUserId,
        ContractFilePurpose purpose,
        Guid relatedEntityId,
        CancellationToken cancellationToken)
    {
        var access = purpose switch
        {
            ContractFilePurpose.ContractAttachment =>
                await dbContext.Contracts
                    .AsNoTracking()
                    .Where(contract => contract.Id == relatedEntityId)
                    .Select(contract => new WriteAccessFacts(
                        contract.Id,
                        contract.ClientUserId,
                        contract.LawyerUserId,
                        false))
                    .SingleOrDefaultAsync(cancellationToken),
            ContractFilePurpose.MilestoneSubmission =>
                await (
                    from milestone in dbContext.Milestones.AsNoTracking()
                    join contract in dbContext.Contracts.AsNoTracking()
                        on milestone.ContractId equals contract.Id
                    where milestone.Id == relatedEntityId
                    select new WriteAccessFacts(
                        contract.Id,
                        contract.ClientUserId,
                        contract.LawyerUserId,
                        true))
                    .SingleOrDefaultAsync(cancellationToken),
            ContractFilePurpose.DisputeEvidence =>
                await FindDisputeWriteAccessAsync(
                    relatedEntityId,
                    cancellationToken),
            _ => null
        };
        if (access is null)
        {
            throw new NotFoundException(
                "السجل المرتبط بالملف غير موجود.");
        }

        var authorized = access.LawyerOnly
            ? actorUserId == access.LawyerUserId
            : actorUserId == access.ClientUserId
                || actorUserId == access.LawyerUserId;
        if (!authorized)
        {
            throw new ForbiddenAccessException(
                "لا يملك المستخدم الحالي صلاحية إرفاق ملفات بهذا السجل.");
        }

        return access;
    }

    private async Task<WriteAccessFacts?> FindDisputeWriteAccessAsync(
        Guid disputeId,
        CancellationToken cancellationToken)
    {
        var trackedDispute = dbContext.Disputes.Local
            .SingleOrDefault(dispute => dispute.Id == disputeId);
        if (trackedDispute is not null)
        {
            return await dbContext.Contracts
                .AsNoTracking()
                .Where(contract => contract.Id == trackedDispute.ContractId)
                .Select(contract => new WriteAccessFacts(
                    contract.Id,
                    contract.ClientUserId,
                    contract.LawyerUserId,
                    false))
                .SingleOrDefaultAsync(cancellationToken);
        }

        return await (
            from dispute in dbContext.Disputes.AsNoTracking()
            join contract in dbContext.Contracts.AsNoTracking()
                on dispute.ContractId equals contract.Id
            where dispute.Id == disputeId
            select new WriteAccessFacts(
                contract.Id,
                contract.ClientUserId,
                contract.LawyerUserId,
                false))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<ReadAccessFacts?> FindReadFactsAsync(
        Guid storedFileId,
        ContractFilePurpose purpose,
        Guid relatedEntityId,
        CancellationToken cancellationToken)
    {
        return purpose switch
        {
            ContractFilePurpose.ContractAttachment =>
                await (
                    from attachment in dbContext.ContractAttachments
                        .AsNoTracking()
                    join contract in dbContext.Contracts.AsNoTracking()
                        on attachment.ContractId equals contract.Id
                    join file in dbContext.StoredFiles.AsNoTracking()
                        on attachment.StoredFileId equals file.Id
                    where attachment.ContractId == relatedEntityId
                        && attachment.StoredFileId == storedFileId
                        && !file.IsDeleted
                    select new ReadAccessFacts(
                        contract.ClientUserId,
                        contract.LawyerUserId,
                        file.FileUrl))
                    .SingleOrDefaultAsync(cancellationToken),
            ContractFilePurpose.MilestoneSubmission =>
                await (
                    from attachment in dbContext.MilestoneSubmissionAttachments
                        .AsNoTracking()
                    join submission in dbContext.MilestoneSubmissions
                            .AsNoTracking()
                        on attachment.MilestoneSubmissionId equals submission.Id
                    join milestone in dbContext.Milestones.AsNoTracking()
                        on submission.MilestoneId equals milestone.Id
                    join contract in dbContext.Contracts.AsNoTracking()
                        on milestone.ContractId equals contract.Id
                    join file in dbContext.StoredFiles.AsNoTracking()
                        on attachment.StoredFileId equals file.Id
                    where milestone.Id == relatedEntityId
                        && attachment.StoredFileId == storedFileId
                        && !file.IsDeleted
                    select new ReadAccessFacts(
                        contract.ClientUserId,
                        contract.LawyerUserId,
                        file.FileUrl))
                    .FirstOrDefaultAsync(cancellationToken),
            ContractFilePurpose.DisputeEvidence =>
                await (
                    from evidence in dbContext.DisputeEvidence.AsNoTracking()
                    join dispute in dbContext.Disputes.AsNoTracking()
                        on evidence.DisputeId equals dispute.Id
                    join contract in dbContext.Contracts.AsNoTracking()
                        on dispute.ContractId equals contract.Id
                    join file in dbContext.StoredFiles.AsNoTracking()
                        on evidence.StoredFileId equals file.Id
                    where dispute.Id == relatedEntityId
                        && evidence.StoredFileId == storedFileId
                        && !file.IsDeleted
                    select new ReadAccessFacts(
                        contract.ClientUserId,
                        contract.LawyerUserId,
                        file.FileUrl))
                    .SingleOrDefaultAsync(cancellationToken),
            _ => null
        };
    }

    private static void ValidateRequest(
        Guid actorUserId,
        IReadOnlyCollection<Guid> storedFileIds,
        ContractFilePurpose purpose,
        Guid relatedEntityId)
    {
        if (actorUserId == Guid.Empty)
        {
            throw new BusinessException(
                "معرّف المستخدم مطلوب للتحقق من صلاحية الملف.");
        }

        if (relatedEntityId == Guid.Empty)
        {
            throw new BusinessException(
                "معرّف السجل المرتبط بالملف مطلوب.");
        }

        if (!Enum.IsDefined(purpose))
        {
            throw new BusinessException(
                "الغرض من استخدام الملف غير صالح.");
        }

        if (storedFileIds.Count == 0
            || storedFileIds.Any(fileId => fileId == Guid.Empty)
            || storedFileIds.Distinct().Count() != storedFileIds.Count)
        {
            throw new BusinessException(
                "يجب تحديد قائمة ملفات صحيحة دون تكرار.");
        }
    }

    private sealed record WriteAccessFacts(
        Guid ContractId,
        Guid ClientUserId,
        Guid LawyerUserId,
        bool LawyerOnly);

    private sealed record ReadAccessFacts(
        Guid ClientUserId,
        Guid LawyerUserId,
        string StoragePath);
}
