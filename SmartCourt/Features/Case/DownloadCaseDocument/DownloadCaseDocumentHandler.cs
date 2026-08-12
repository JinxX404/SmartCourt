using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Case.DownloadCaseDocument;

public class DownloadCaseDocumentHandler : IRequestHandler<DownloadCaseDocumentQuery, DownloadCaseDocumentResult>
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public DownloadCaseDocumentHandler(ApplicationDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<DownloadCaseDocumentResult> Handle(DownloadCaseDocumentQuery request, CancellationToken cancellationToken)
    {
        var caseDocument = await _context.CaseDocuments
            .Include(cd => cd.StoredFile)
            .FirstOrDefaultAsync(cd => cd.CaseId == request.CaseId && cd.StoredFileId == request.DocumentId, cancellationToken);

        if (caseDocument is null)
            throw new BusinessException("Document not found for the specified case.");

        var storedFile = caseDocument.StoredFile;

        var fileBytes = await _fileStorageService.DownloadAsync(storedFile.FileUrl, cancellationToken);

        return new DownloadCaseDocumentResult
        {
            FileBytes = fileBytes,
            ContentType = storedFile.ContentType,
            FileName = storedFile.OriginalFileName
        };
    }
}
