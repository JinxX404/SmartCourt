using SmartCourt.Common.Models;
﻿using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Entities;
using SmartCourt.Features.UserVerification.SubmitVerificationDocuments.DTOs;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using SmartCourt.Features.Auth.Enums;
using Supabase.Gotrue;

namespace SmartCourt.Features.UserVerification.SubmitVerificationDocuments
{
    public class SubmitVerificationDocumentsHandler : IRequestHandler<SubmitVerificationDocumentsCommand, ApiResponse<SubmitVerificationDocumentResponseDto>>
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IValidator<SubmitVerificationDocumentsCommand> _validator;
        private readonly IFileStorageService _fileStorageService;

        private static readonly string[] AllowedImageContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/heic",
            "image/heif"
        };

        public SubmitVerificationDocumentsHandler(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IValidator<SubmitVerificationDocumentsCommand> validator,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _userManager = userManager;
            _validator = validator;
            _fileStorageService = fileStorageService;
        }

        public async Task<ApiResponse<SubmitVerificationDocumentResponseDto>> Handle(SubmitVerificationDocumentsCommand request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid)
                return ApiResponse<SubmitVerificationDocumentResponseDto>
                    .Fail(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), 400);

            var user = await _context.Users
                .Include(u => u.VerificationDocuments)
                .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null)
                return ApiResponse<SubmitVerificationDocumentResponseDto>
                    .Fail(new List<string> { "The specified user doesn't exists" });

            var responseDto = new SubmitVerificationDocumentResponseDto();
            var uploadedPaths = new List<string>();

            var pendingTypes = await _context.UserVerificationDocuments
                .Where(d => d.UserId == request.UserId &&
                d.Status == VerificationDocumentStatus.Pending)
                .Select(d => d.DocumentType)
                .Distinct()
                .ToListAsync();

            foreach (var document in request.Documents)
            {
                if (document is null)
                {
                    responseDto.FailedDocuments.Add(new DocumentUploadErrorDto
                    {
                        FileName = string.Empty,
                        Error = "Document is null.",
                        Type = VerificationDocumentType.Other
                    });

                    continue;
                }
                
                if(document.File.Length == 0)
                {
                    responseDto.FailedDocuments.Add(new DocumentUploadErrorDto
                    {
                        FileName = document.File.FileName,
                        Error = "Document is empty.",
                        Type = document.Type
                    });

                    continue;
                }

                if(!AllowedImageContentTypes.Contains(document.File.ContentType))
                {
                    responseDto.FailedDocuments.Add(new DocumentUploadErrorDto
                    {
                        FileName = document.File.FileName,
                        Error = "Only JPEG, PNG, WEBP, HEIC, and HEIF images are allowed.",
                        Type = document.Type
                    });

                    continue;
                }

                if(document.ExpirationDate <= DateOnly.FromDateTime(DateTime.Today))
                {
                    responseDto.FailedDocuments.Add(new DocumentUploadErrorDto
                    {
                        FileName = document.File.FileName,
                        Error = "This document is expired",
                        Type = document.Type
                    });

                    continue;
                }

                // Allowed replacing documents anytime
                
                try
                {
                    string folder = document.Type switch
                    {
                        VerificationDocumentType.NationalIdFront or
                        VerificationDocumentType.NationalIdBack 
                        => "national-id",
                
                        VerificationDocumentType.BarAssociationCardFront or
                        VerificationDocumentType.BarAssociationCardBack 
                        => "bar-membership",
                
                        VerificationDocumentType.SelfieWithId => "selfie",
                        VerificationDocumentType.Other => "other",

                        _ => throw new InvalidOperationException("Unsupported verification document type.")
                    };
                
                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(document.File.FileName)}";
                    string filePath = $"{request.UserId}/{folder}/{fileName}";
                
                    await using var stream = document.File.OpenReadStream();
                
                    var uploadResult = await _fileStorageService.UploadAsync(
                        stream,
                        filePath,
                        document.File.FileName,
                        cancellationToken);

                    uploadedPaths.Add(uploadResult.StoragePath);

                    responseDto.UploadedDocuments.Add(new UploadedDocumentDto
                    {
                        FileName = document.File.FileName,
                        Type = document.Type,
                    });

                    StoredFile file = new()
                    {
                        StoredFileName = fileName,
                        OriginalFileName = document.File.FileName,
                        ContentType = document.File.ContentType,
                        Extension = Path.GetExtension(document.File.FileName),
                        SizeInBytes = document.File.Length,
                        FileUrl = uploadResult.StoragePath,
                    };

                    _context.StoredFiles.Add(file);

                    UserVerificationDocument verificationDocument = new()
                    {
                        DocumentType = document.Type,
                        IsCurrent = true,
                        StoredFile = file,
                        User = user,
                        ExpirationDate = document.ExpirationDate,
                        Status = VerificationDocumentStatus.Pending
                    };

                    foreach (var previousVersion in user.VerificationDocuments.Where(d => 
                                 d.DocumentType == document.Type && 
                                 d.IsCurrent))
                    {
                        previousVersion.IsCurrent = false;
                    }

                    _context.UserVerificationDocuments.Add(verificationDocument);
                }
                catch (Exception ex)
                {
                    responseDto.FailedDocuments.Add(new DocumentUploadErrorDto
                    {
                        FileName = document.File.FileName,
                        Error = $"An error occurred while uploading the document: {ex.Message}",
                        Type = document.Type
                    });
                }
            }

            try
            {
                if (responseDto.UploadedDocuments.Count > 0)
                {
                    user.Status = UserStatus.PendingReview;
                    _context.Users.Update(user);
                }

                if(_context.ChangeTracker.HasChanges())
                    await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                foreach (var filePath in uploadedPaths)
                {
                    await _fileStorageService.DeleteAsync(filePath, cancellationToken);
                }

                return ApiResponse<SubmitVerificationDocumentResponseDto>
                    .Fail(new List<string> { "An error occured while uploading your documents. Try again please.." });
            }

            return ApiResponse<SubmitVerificationDocumentResponseDto>.Ok(responseDto);
        }
    }
}
