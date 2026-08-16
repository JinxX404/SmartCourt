using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Contracts.Files;

public sealed class ContractFileService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IFileStorageService fileStorageService,
    TimeProvider timeProvider,
    ILogger<ContractFileService> logger) : IContractFileService
{
    public async Task<IReadOnlyList<ContractFileDto>> UploadAsync(
        Guid contractId,
        UploadContractFilesRequest request,
        CancellationToken cancellationToken)
    {
        if (contractId == Guid.Empty)
        {
            throw new BusinessException("معرّف العقد مطلوب.");
        }

        var actorUserId = GetActorUserId();
        await EnsureParticipantAsync(
            contractId,
            actorUserId,
            cancellationToken);

        var inspectedFiles =
            new List<(IFormFile File, ContractFileInspection Inspection)>();
        foreach (var file in request.Files)
        {
            var inspection = await ContractFileUploadPolicy.InspectAsync(
                file,
                cancellationToken);
            if (!inspection.IsValid)
            {
                throw new BusinessException(
                    $"{Path.GetFileName(file.FileName)}: {inspection.Error}");
            }

            inspectedFiles.Add((file, inspection));
        }

        var now = timeProvider.GetUtcNow();
        var uploadedPaths = new List<string>();
        var result = new List<ContractFileDto>(inspectedFiles.Count);

        try
        {
            foreach (var (file, inspection) in inspectedFiles)
            {
                var storedFileId = Guid.NewGuid();
                var storagePath = string.Join(
                    '/',
                    "contract-files",
                    contractId.ToString("N"),
                    actorUserId.ToString("N"),
                    $"{storedFileId:N}{inspection.Extension}");

                await using var stream = file.OpenReadStream();
                var upload = await fileStorageService.UploadAsync(
                    stream,
                    storagePath,
                    inspection.SafeFileName!,
                    inspection.ContentType,
                    cancellationToken);
                uploadedPaths.Add(upload.StoragePath);

                var storedFile = new StoredFile
                {
                    Id = storedFileId,
                    StoredFileName = Path.GetFileName(storagePath),
                    OriginalFileName = inspection.SafeFileName!,
                    FileUrl = upload.StoragePath,
                    ContentType = inspection.ContentType!,
                    Extension = inspection.Extension!,
                    SizeInBytes = upload.Size
                };
                var attachment = new ContractAttachment(
                    Guid.NewGuid(),
                    contractId,
                    storedFileId,
                    actorUserId,
                    now);

                dbContext.StoredFiles.Add(storedFile);
                dbContext.ContractAttachments.Add(attachment);
                result.Add(new ContractFileDto(
                    storedFileId,
                    inspection.SafeFileName!,
                    inspection.ContentType!,
                    upload.Size,
                    now));
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return result;
        }
        catch
        {
            foreach (var storagePath in uploadedPaths)
            {
                try
                {
                    await fileStorageService.DeleteAsync(
                        storagePath,
                        CancellationToken.None);
                }
                catch (Exception cleanupException)
                {
                    logger.LogWarning(
                        cleanupException,
                        "Could not clean up contract file {StoragePath} after upload failure.",
                        storagePath);
                }
            }

            throw;
        }
    }

    public async Task DeleteAsync(
        Guid contractId,
        Guid storedFileId,
        CancellationToken cancellationToken)
    {
        if (contractId == Guid.Empty || storedFileId == Guid.Empty)
        {
            throw new BusinessException(
                "معرّف العقد ومعرّف الملف مطلوبان.");
        }

        var actorUserId = GetActorUserId();
        await EnsureParticipantAsync(
            contractId,
            actorUserId,
            cancellationToken);

        var fileRecord = await (
            from attachment in dbContext.ContractAttachments
            join file in dbContext.StoredFiles
                on attachment.StoredFileId equals file.Id
            where attachment.ContractId == contractId
                && attachment.StoredFileId == storedFileId
                && !file.IsDeleted
            select new
            {
                Attachment = attachment,
                File = file
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (fileRecord is null)
        {
            throw new NotFoundException("الملف المرتبط بالعقد غير موجود.");
        }

        if (fileRecord.Attachment.UploadedByUserId != actorUserId)
        {
            throw new ForbiddenAccessException(
                "لا يمكن حذف ملف رفعه طرف آخر في العقد.");
        }

        var isReferenced = await dbContext.MilestoneSubmissionAttachments
                .AsNoTracking()
                .AnyAsync(
                    attachment => attachment.StoredFileId == storedFileId,
                    cancellationToken)
            || await dbContext.DisputeEvidence
                .AsNoTracking()
                .AnyAsync(
                    evidence => evidence.StoredFileId == storedFileId,
                    cancellationToken);
        if (isReferenced)
        {
            throw new ConflictException(
                "لا يمكن حذف ملف مستخدم في تسليم مرحلة أو دليل نزاع.");
        }

        dbContext.ContractAttachments.Remove(fileRecord.Attachment);
        dbContext.StoredFiles.Remove(fileRecord.File);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await fileStorageService.DeleteAsync(
                fileRecord.File.FileUrl,
                cancellationToken);
        }
        catch (Exception cleanupException)
        {
            logger.LogWarning(
                cleanupException,
                "Could not remove deleted contract file {StoragePath} from storage.",
                fileRecord.File.FileUrl);
        }
    }

    private async Task EnsureParticipantAsync(
        Guid contractId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var participant = await dbContext.Contracts
            .AsNoTracking()
            .Where(contract => contract.Id == contractId)
            .Select(contract => new
            {
                contract.ClientUserId,
                contract.LawyerUserId
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (participant is null)
        {
            throw new NotFoundException("العقد المطلوب غير موجود.");
        }

        if (participant.ClientUserId != actorUserId
            && participant.LawyerUserId != actorUserId)
        {
            throw new ForbiddenAccessException(
                "لا يملك المستخدم الحالي صلاحية إدارة ملفات هذا العقد.");
        }
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
}
