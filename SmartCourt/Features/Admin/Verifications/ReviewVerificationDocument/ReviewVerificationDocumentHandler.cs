using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Extensions;
using SmartCourt.Features.Admin.Verifications.Shared;
using SmartCourt.Features.Admin.Verifications.ReviewVerificationDocument.DTOs;
using SmartCourt.Entities;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Admin.Verifications.ReviewVerificationDocument;

public sealed class ReviewVerificationDocumentHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager,
    IValidator<ReviewVerificationDocumentCommand> validator,
    SmartCourt.Features.Notifications.Services.INotificationsService notificationsService)
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

        // Admin can review current documents (Pending or previously Verified/Rejected)

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (document.ExpirationDate <= today)
        {
            document.Status = VerificationDocumentStatus.Expired;
            var isLawyerExpired = await userManager.IsInRoleAsync(document.User, "Lawyer");
            document.User.Status = VerificationStatusEvaluator.ResolveAccountStatus(
                document.User.VerificationDocuments,
                today,
                isLawyerExpired);
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
        }
        else
        {
            document.Status = VerificationDocumentStatus.Rejected;
            document.VerifiedAt = null;
            document.VerifiedByAdminId = adminId;
            document.RejectionReason = request.RejectionReason!.Trim();
        }

        // Demote any other current document of the same type that was previously
        // marked current (handles the replacement-document scenario).
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
            isLawyer);

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

        // Send notification
        var docNameAr = document.DocumentType switch
        {
            VerificationDocumentType.NationalIdFront => "صورة البطاقة (الأمام)",
            VerificationDocumentType.NationalIdBack => "صورة البطاقة (الخلف)",
            VerificationDocumentType.BarAssociationCardFront => "كارنيه النقابة (الأمام)",
            VerificationDocumentType.BarAssociationCardBack => "كارنيه النقابة (الخلف)",
            VerificationDocumentType.SelfieWithId => "الصورة الشخصية مع البطاقة",
            _ => "المستند"
        };

        if (request.Decision == VerificationReviewDecision.Approve)
        {
            await notificationsService.SendNotificationAsync(
                document.UserId,
                "تم قبول المستند",
                $"تم قبول {docNameAr} الخاص بك بنجاح.",
                cancellationToken);
        }
        else
        {
            await notificationsService.SendNotificationAsync(
                document.UserId,
                "تم رفض المستند",
                $"تم رفض {docNameAr} الخاص بك. السبب: {request.RejectionReason}",
                cancellationToken);
        }

        return ApiResponse<ReviewVerificationDocumentResponse>.Ok(new ReviewVerificationDocumentResponse
        {
            DocumentId = document.Id,
            DocumentStatus = document.Status.ToString(),
            LawyerAccountStatus = document.User.Status.ToString(),
            IsFullyVerified = isFullyVerified
        });
    }
}
