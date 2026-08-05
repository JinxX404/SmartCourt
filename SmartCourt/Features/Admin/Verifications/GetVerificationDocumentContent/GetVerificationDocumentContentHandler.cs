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

        // Return a URL instead of streaming raw bytes through the web server.
        // Swap GetDownloadUrlAsync for CreateSignedUrlAsync when the Supabase
        // bucket is switched to private — the handler stays unchanged.
        var downloadUrl = await fileStorageService.GetDownloadUrlAsync(
            document.StoredFile.FileUrl, cancellationToken);

        return ApiResponse<VerificationDocumentContentDto>.Ok(new VerificationDocumentContentDto
        {
            DownloadUrl = downloadUrl,
            ContentType = document.StoredFile.ContentType,
            FileName = document.StoredFile.OriginalFileName
        });
    }
}
