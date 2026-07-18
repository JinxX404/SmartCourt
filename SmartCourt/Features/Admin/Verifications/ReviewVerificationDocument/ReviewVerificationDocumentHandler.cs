using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Models;
using SmartCourt.Extensions;
using SmartCourt.Features.Admin.Verifications.Shared;
using SmartCourt.Features.Admin.Verifications.ReviewVerificationDocument.DTOs;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Entities;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Admin.Verifications.ReviewVerificationDocument;

public sealed class ReviewVerificationDocumentHandler(
    ApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor,
    UserManager<ApplicationUser> userManager,
    IValidator<ReviewVerificationDocumentCommand> validator)
    : IRequestHandler<ReviewVerificationDocumentCommand, ApiResponse<ReviewVerificationDocumentResponse>>
{
    public async Task<ApiResponse<ReviewVerificationDocumentResponse>> Handle(
        ReviewVerificationDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ApiResponse<ReviewVerificationDocumentResponse>.Fail(
                validationResult.Errors.Select(error => error.ErrorMessage).ToList());
        }

        var document = await context.UserVerificationDocuments
            .Include(verificationDocument => verificationDocument.User)
            .ThenInclude(user => user.VerificationDocuments)
            .SingleOrDefaultAsync(verificationDocument => verificationDocument.Id == request.DocumentId, cancellationToken);

        if (document is null)
        {
            return ApiResponse<ReviewVerificationDocumentResponse>.Fail("Verification document was not found.", 404);
        }

        if (!await userManager.IsInRoleAsync(document.User, "Lawyer"))
        {
            return ApiResponse<ReviewVerificationDocumentResponse>.Fail("Verification document was not found.", 404);
        }

        if (!document.IsCurrent)
        {
            return ApiResponse<ReviewVerificationDocumentResponse>.Fail(
                "Only the current version of a document can be reviewed.",
                StatusCodes.Status409Conflict);
        }

        if (document.Status != VerificationDocumentStatus.Pending)
        {
            return ApiResponse<ReviewVerificationDocumentResponse>.Fail(
                "Only pending documents can be reviewed.",
                StatusCodes.Status409Conflict);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (document.ExpirationDate <= today)
        {
            document.Status = VerificationDocumentStatus.Expired;
            document.User.Status = VerificationStatusEvaluator.ResolveAccountStatus(
                document.User.VerificationDocuments,
                today);
            await context.SaveChangesAsync(cancellationToken);

            return ApiResponse<ReviewVerificationDocumentResponse>.Fail(
                "The document has expired and must be submitted again.",
                StatusCodes.Status409Conflict);
        }

        if (request.Decision == VerificationReviewDecision.Approve)
        {
            document.Status = VerificationDocumentStatus.Verified;
            document.VerifiedAt = DateTime.UtcNow;
            document.VerifiedByAdminId = httpContextAccessor.HttpContext!.User.GetUserId();
            document.RejectionReason = null;
        }
        else
        {
            document.Status = VerificationDocumentStatus.Rejected;
            document.VerifiedAt = null;
            document.VerifiedByAdminId = httpContextAccessor.HttpContext!.User.GetUserId();
            document.RejectionReason = request.RejectionReason!.Trim();
        }

        // Keep one current document per type once a replacement has been reviewed.
        foreach (var previousVersion in document.User.VerificationDocuments.Where(candidate =>
                     candidate.Id != document.Id &&
                     candidate.DocumentType == document.DocumentType &&
                     candidate.IsCurrent))
        {
            previousVersion.IsCurrent = false;
        }

        document.User.Status = VerificationStatusEvaluator.ResolveAccountStatus(
            document.User.VerificationDocuments,
            today);

        await context.SaveChangesAsync(cancellationToken);

        return ApiResponse<ReviewVerificationDocumentResponse>.Ok(new ReviewVerificationDocumentResponse
        {
            DocumentId = document.Id,
            DocumentStatus = document.Status.ToString(),
            LawyerAccountStatus = document.User.Status.ToString(),
            IsFullyVerified = document.User.Status == UserStatus.Active
        });
    }
}
