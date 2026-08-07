using SmartCourt.Common.Models;
﻿using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Entities;
using SmartCourt.Features.UserVerification.GetUserVerificationDocuments.DTOs;
using SmartCourt.Persistence;

namespace SmartCourt.Features.UserVerification.GetUserVerificationDocuments
{
    public sealed class GetUserVerificationDocumentsQueryHandler : IRequestHandler<GetUserVerificationDocumentsQuery, ApiResponse<GetUserVerificationDocumentsResponseDto>>
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IValidator<GetUserVerificationDocumentsQuery> _validator;

        public GetUserVerificationDocumentsQueryHandler(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IValidator<GetUserVerificationDocumentsQuery> validator)
        {
            _context = context;
            _userManager = userManager;
            _validator = validator;
        }

        public async Task<ApiResponse<GetUserVerificationDocumentsResponseDto>> Handle(
            GetUserVerificationDocumentsQuery request,
            CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid)
                return ApiResponse<GetUserVerificationDocumentsResponseDto>
                    .Fail(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), 400);

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
                return ApiResponse<GetUserVerificationDocumentsResponseDto>
                    .Fail(new List<string>(){ "The specified user does not exist." }, 404);

            var documents = await _context.UserVerificationDocuments
                .AsNoTracking()
                .Where(d => d.UserId == request.UserId)
                .Select(d => new UserVerificationDocumentDto
                {
                    DocumentId = d.Id,
                    DocumentType = d.DocumentType,
                    Status = d.Status,
                    ExpirationDate = d.ExpirationDate,
                    IsCurrent = d.IsCurrent,
                    FileName = d.StoredFile.OriginalFileName,
                    RejectionReason = d.RejectionReason
                })
                .ToListAsync();

            return ApiResponse<GetUserVerificationDocumentsResponseDto>
                .Ok(new GetUserVerificationDocumentsResponseDto()
                {
                    Documents = documents
                });
        }
    }
}
