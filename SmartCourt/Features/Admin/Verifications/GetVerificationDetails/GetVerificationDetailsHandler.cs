using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using SmartCourt.Features.Admin.Verifications.GetVerificationDetails.DTOs;
using SmartCourt.Features.Admin.Verifications.Shared;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Admin.Verifications.GetVerificationDetails;

public sealed class GetVerificationDetailsHandler(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<GetVerificationDetailsQuery, ApiResponse<VerificationDetailsDto>>
{
    public async Task<ApiResponse<VerificationDetailsDto>> Handle(
        GetVerificationDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var lawyer = await context.Users
            .Include(user => user.VerificationDocuments.Where(document => document.IsCurrent))
            .ThenInclude(document => document.StoredFile)
            .SingleOrDefaultAsync(user => user.Id == request.LawyerId, cancellationToken);

        if (lawyer is null)
        {
            throw new NotFoundException("Lawyer was not found.");
        }

        if (!await userManager.IsInRoleAsync(lawyer, "Lawyer"))
        {
            throw new NotFoundException("Lawyer was not found.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var documents = lawyer.VerificationDocuments
            .OrderBy(document => document.DocumentType)
            .Select(document => new VerificationDocumentDetailsDto
            {
                DocumentId = document.Id,
                DocumentType = document.DocumentType.ToString(),
                Status = document.Status.ToString(),
                FileName = document.StoredFile.OriginalFileName,
                ContentType = document.StoredFile.ContentType,
                ExpirationDate = document.ExpirationDate,
                ReviewedAt = document.VerifiedAt,
                RejectionReason = document.RejectionReason,
                ContentUrl = $"/api/admin/verifications/documents/{document.Id}/content"
            })
            .ToList();

        // Fix: compute IsFullyVerified from actual document state.
        // Deriving this from UserStatus.Active is incorrect — an Active seeded
        // lawyer with no documents would be reported as fully verified.
        var isFullyVerified = VerificationStatusEvaluator.IsFullyVerified(
            lawyer.VerificationDocuments, today);

        return ApiResponse<VerificationDetailsDto>.Ok(new VerificationDetailsDto
        {
            LawyerId = lawyer.Id,
            FullName = lawyer.FullName,
            Email = lawyer.Email ?? string.Empty,
            PhoneNumber = lawyer.PhoneNumber,
            AccountStatus = lawyer.Status.ToString(),
            IsFullyVerified = isFullyVerified,
            Documents = documents
        });
    }
}
