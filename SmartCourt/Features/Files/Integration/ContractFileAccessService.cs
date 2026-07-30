using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Files.Integration;

public sealed class ContractFileAccessService(
    ApplicationDbContext dbContext,
    IFileStorageService fileStorageService,
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

        var requestedIds = storedFileIds.Distinct().ToArray();
        var authorizedIds = await dbContext.UserVerificationDocuments
            .AsNoTracking()
            .Where(document =>
                document.UserId == actorUserId
                && !document.IsDeleted
                && requestedIds.Contains(document.StoredFileId)
                && !document.StoredFile.IsDeleted)
            .Select(document => document.StoredFileId)
            .Distinct()
            .ToListAsync(cancellationToken);
        if (authorizedIds.Count != requestedIds.Length)
        {
            throw new ForbiddenAccessException(
                "أحد الملفات المحددة غير موجود أو لا يملكه المستخدم الحالي، لذلك لا يمكن إرفاقه.");
        }

        return authorizedIds
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

        var storagePath = await dbContext.UserVerificationDocuments
            .AsNoTracking()
            .Where(document =>
                document.UserId == actorUserId
                && document.StoredFileId == storedFileId
                && !document.IsDeleted
                && !document.StoredFile.IsDeleted)
            .Select(document => document.StoredFile.FileUrl)
            .FirstOrDefaultAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return null;
        }

        var url = await fileStorageService.GetDownloadUrlAsync(
            storagePath,
            cancellationToken);
        if (!Uri.TryCreate(url, UriKind.Absolute, out var signedUri))
        {
            throw new BusinessException(
                "تعذر إنشاء رابط آمن لقراءة الملف المطلوب.");
        }

        return new ContractFileReadAccess(
            storedFileId,
            signedUri,
            timeProvider.GetUtcNow().AddMinutes(5));
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
}
