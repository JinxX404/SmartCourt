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
            .Include(user => user.LawyerProfile)
            .ThenInclude(lp => lp!.Specializations)
            .SingleOrDefaultAsync(user => user.Id == request.LawyerId, cancellationToken);

        if (lawyer is null)
        {
            throw new NotFoundException("User was not found.");
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

        var roles = await userManager.GetRolesAsync(lawyer);
        var primaryRole = roles.FirstOrDefault();
        var isLawyer = primaryRole == "Lawyer";

        var isFullyVerified = VerificationStatusEvaluator.IsFullyVerified(
            lawyer.VerificationDocuments, today, isLawyer);

        var spec = lawyer.LawyerProfile?.Specializations.FirstOrDefault();
        string? specName = spec?.Specialization.ToString();

        return ApiResponse<VerificationDetailsDto>.Ok(new VerificationDetailsDto
        {
            LawyerId = lawyer.Id,
            FullName = lawyer.FullName,
            Email = lawyer.Email ?? string.Empty,
            PhoneNumber = lawyer.PhoneNumber,
            NationalNumber = lawyer.NationalNumber,
            Address = lawyer.Address,
            DateOfBirth = lawyer.DateOfBirth,
            AccountStatus = lawyer.Status.ToString(),
            IsFullyVerified = isFullyVerified,
            Role = primaryRole,
            Level = lawyer.LawyerProfile != null ? (int)lawyer.LawyerProfile.Level : null,
            SpecializationName = specName,
            YearsOfExperience = spec?.YearsOfExperience,
            Bio = lawyer.LawyerProfile?.Bio,
            Documents = documents
        });
    }
}
