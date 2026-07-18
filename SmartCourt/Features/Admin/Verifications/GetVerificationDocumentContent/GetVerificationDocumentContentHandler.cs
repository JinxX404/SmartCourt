using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Admin.Verifications.GetVerificationDocumentContent.DTOs;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Admin.Verifications.GetVerificationDocumentContent;

public sealed class GetVerificationDocumentContentHandler(
    ApplicationDbContext context,
    IFileStorageService fileStorageService)
    : IRequestHandler<GetVerificationDocumentContentQuery, ApiResponse<VerificationDocumentContentDto>>
{
    public async Task<ApiResponse<VerificationDocumentContentDto>> Handle(
        GetVerificationDocumentContentQuery request,
        CancellationToken cancellationToken)
    {
        if (request.DocumentId == Guid.Empty)
        {
            return ApiResponse<VerificationDocumentContentDto>.Fail("Document id is required.");
        }

        var document = await context.UserVerificationDocuments
            .AsNoTracking()
            .Include(verificationDocument => verificationDocument.StoredFile)
            .SingleOrDefaultAsync(verificationDocument =>
                verificationDocument.Id == request.DocumentId && verificationDocument.IsCurrent,
                cancellationToken);

        if (document is null)
        {
            throw new NotFoundException("Verification document was not found.");
        }

        var content = await fileStorageService.DownloadAsync(document.StoredFile.FileUrl, cancellationToken);
        return ApiResponse<VerificationDocumentContentDto>.Ok(new VerificationDocumentContentDto
        {
            Content = content,
            ContentType = document.StoredFile.ContentType,
            FileName = document.StoredFile.OriginalFileName
        });
    }
}
