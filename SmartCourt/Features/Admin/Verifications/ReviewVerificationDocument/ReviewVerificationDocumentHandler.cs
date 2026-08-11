using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Extensions;
using SmartCourt.Features.Admin.Verifications.Events;
using SmartCourt.Features.Admin.Verifications.Shared;
using SmartCourt.Features.Admin.Verifications.ReviewVerificationDocument.DTOs;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Entities;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Infrastructure.Providers.Events;

namespace SmartCourt.Features.Admin.Verifications.ReviewVerificationDocument;

public sealed class ReviewVerificationDocumentHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager,
    IValidator<ReviewVerificationDocumentCommand> validator,
    IFileStorageService fileStorageService,
    IOutboxWriter outboxWriter)
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
            .Include(verificationDocument => verificationDocument.StoredFile)
            .Include(verificationDocument => verificationDocument.User)
            .ThenInclude(user => user.VerificationDocuments)
                .ThenInclude(d => d.StoredFile)
            .SingleOrDefaultAsync(verificationDocument => verificationDocument.Id == request.DocumentId, cancellationToken);

        // Use NotFoundException instead of ApiResponse.Fail so ExceptionHandlingMiddleware
        // renders a consistent 404 ApiResponse<string> shape.
        if (document is null)
        {
            throw new NotFoundException("Verification document was not found.");
        }

        if (!document.IsCurrent)
        {
            throw new ConflictException("Only the current version of a document can be reviewed.");
        }

        var previousDocumentStatus = document.Status;
        var previousAccountStatus = document.User.Status;
        var correlationId = Guid.NewGuid();

        // Admin can review current documents (Pending or previously Verified/Rejected)

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (document.ExpirationDate <= today)
        {
            document.Status = VerificationDocumentStatus.Expired;
            var isLawyerExpired = await userManager.IsInRoleAsync(document.User, "Lawyer");
            document.User.Status = VerificationStatusEvaluator.ResolveAccountStatus(
                document.User.VerificationDocuments,
                today,
                isLawyerExpired,
                document.User.PhoneNumberConfirmed,
                document.User.Status);

            if (previousDocumentStatus != VerificationDocumentStatus.Expired)
            {
                await VerificationOutbox.EnqueueDocumentAsync(
                    outboxWriter,
                    VerificationEventTypes.DocumentExpired,
                    document,
                    correlationId,
                    cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);

            throw new ConflictException("The document has expired and must be submitted again.");
        }

        var adminId = currentUserService.UserId?.ToString()
                      ?? throw new ConflictException("Admin identity could not be resolved.");

        if (request.Decision == VerificationReviewDecision.Approve)
        {
            document.Status = VerificationDocumentStatus.Verified;
            document.VerifiedAt = DateTime.UtcNow;
            document.VerifiedByAdminId = adminId;
            document.RejectionReason = null;

            if (document.DocumentType == VerificationDocumentType.OfficialProfilePicture)
            {
                document.User.ProfilePictureUrl = document.StoredFile.FileUrl;
            }

            // Delete any older documents of the same type now that the new one is approved
            var oldDocuments = document.User.VerificationDocuments
                .Where(candidate => candidate.Id != document.Id && candidate.DocumentType == document.DocumentType)
                .ToList();

            foreach (var oldDoc in oldDocuments)
            {
                if (oldDoc.StoredFile != null)
                {
                    await fileStorageService.DeleteAsync(oldDoc.StoredFile.FileUrl, cancellationToken);
                    context.StoredFiles.Remove(oldDoc.StoredFile);
                }
                context.UserVerificationDocuments.Remove(oldDoc);
            }
        }
        else
        {
            document.Status = VerificationDocumentStatus.Rejected;
            document.VerifiedAt = null;
            document.VerifiedByAdminId = adminId;
            document.RejectionReason = request.RejectionReason!.Trim();
        }

        // We already handled deleting old ones if approved. 
        // If rejected, there might be another IsCurrent document? 
        // SubmitVerificationDocumentsHandler sets IsCurrent = false for the old one immediately.
        // So this loop below is generally redundant now but we can keep it for safety.
        foreach (var previousVersion in document.User.VerificationDocuments.Where(candidate =>
                     candidate.Id != document.Id &&
                     candidate.DocumentType == document.DocumentType &&
                     candidate.IsCurrent))
        {
            previousVersion.IsCurrent = false;
        }

        var isLawyer = await userManager.IsInRoleAsync(document.User, "Lawyer");

        document.User.Status = VerificationStatusEvaluator.ResolveAccountStatus(
            document.User.VerificationDocuments,
            today,
            isLawyer,
            document.User.PhoneNumberConfirmed,
            document.User.Status);

        if (previousDocumentStatus != document.Status)
        {
            var documentEventType = document.Status switch
            {
                VerificationDocumentStatus.Verified =>
                    VerificationEventTypes.DocumentApproved,
                VerificationDocumentStatus.Rejected =>
                    VerificationEventTypes.DocumentRejected,
                _ => null
            };

            if (documentEventType is not null)
            {
                await VerificationOutbox.EnqueueDocumentAsync(
                    outboxWriter,
                    documentEventType,
                    document,
                    correlationId,
                    cancellationToken);
            }
        }

        if (previousAccountStatus != document.User.Status
            && document.User.Status == UserStatus.Active)
        {
            await VerificationOutbox.EnqueueAccountAsync(
                outboxWriter,
                VerificationEventTypes.AccountApproved,
                document.User,
                correlationId,
                cancellationToken);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Two admins reviewed the same document concurrently.
            // The first writer wins; the second gets a 409 with a clear message.
            throw new ConflictException(
                "تم مراجعة هذا المستند بالفعل من قبل مسؤول آخر. يرجى تحديث الصفحة.");
        }

        // Fix: derive IsFullyVerified from actual document state, not account status.
        // An Active seeded lawyer with zero documents would otherwise report as fully verified.
        var isFullyVerified = VerificationStatusEvaluator.IsFullyVerified(
            document.User.VerificationDocuments, today, isLawyer);



        return ApiResponse<ReviewVerificationDocumentResponse>.Ok(new ReviewVerificationDocumentResponse
        {
            DocumentId = document.Id,
            DocumentStatus = document.Status.ToString(),
            LawyerAccountStatus = document.User.Status.ToString(),
            IsFullyVerified = isFullyVerified
        });
    }
}
