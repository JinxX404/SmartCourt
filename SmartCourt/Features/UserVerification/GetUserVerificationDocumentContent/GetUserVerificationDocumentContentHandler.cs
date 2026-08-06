using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.UserVerification.GetUserVerificationDocumentContent.DTOs;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;

namespace SmartCourt.Features.UserVerification.GetUserVerificationDocumentContent;

public sealed class GetUserVerificationDocumentContentHandler(
    ApplicationDbContext context,
    IFileStorageService fileStorageService)
    : IRequestHandler<GetUserVerificationDocumentContentQuery, ApiResponse<UserVerificationDocumentContentDto>>
{
    public async Task<ApiResponse<UserVerificationDocumentContentDto>> Handle(
        GetUserVerificationDocumentContentQuery request,
        CancellationToken cancellationToken)
    {
        var document = await context.UserVerificationDocuments
            .AsNoTracking()
            .Include(verificationDocument => verificationDocument.StoredFile)
            .SingleOrDefaultAsync(verificationDocument =>
                verificationDocument.Id == request.DocumentId && 
                verificationDocument.UserId == request.UserId &&
                verificationDocument.IsCurrent,
                cancellationToken);

        if (document is null)
        {
            throw new NotFoundException("Verification document was not found or access denied.");
        }

        var downloadUrl = await fileStorageService.GetDownloadUrlAsync(
            document.StoredFile.FileUrl, cancellationToken);

        return ApiResponse<UserVerificationDocumentContentDto>.Ok(new UserVerificationDocumentContentDto
        {
            DownloadUrl = downloadUrl,
            ContentType = document.StoredFile.ContentType,
            FileName = document.StoredFile.OriginalFileName
        });
    }
}
