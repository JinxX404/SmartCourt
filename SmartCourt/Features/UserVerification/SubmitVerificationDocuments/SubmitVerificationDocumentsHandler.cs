using SmartCourt.Common.Models;
﻿using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Entities;
using SmartCourt.Features.Admin.Verifications.Events;
using SmartCourt.Features.UserVerification.SubmitVerificationDocuments.DTOs;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;
using SmartCourt.Features.Auth.Enums;
using Supabase.Gotrue;

namespace SmartCourt.Features.UserVerification.SubmitVerificationDocuments
{
    public class SubmitVerificationDocumentsHandler : IRequestHandler<SubmitVerificationDocumentsCommand, ApiResponse<SubmitVerificationDocumentResponseDto>>
    {
        private record DocumentProcessingResult(
            bool Success,
            DocumentUploadErrorDto? Error = null,
            FileUploadResult? UploadResult = null,
            StoredFile? File = null,
            UserVerificationDocument? VerificationDocument = null,
            UploadedDocumentDto? ResponseItem = null
        );

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IValidator<SubmitVerificationDocumentsCommand> _validator;
        private readonly IFileStorageService _fileStorageService;
        private readonly IOutboxWriter _outboxWriter;

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
            IFileStorageService fileStorageService,
            IOutboxWriter outboxWriter)
        {
            _context = context;
            _userManager = userManager;
            _validator = validator;
            _fileStorageService = fileStorageService;
            _outboxWriter = outboxWriter;
        }

        public async Task<ApiResponse<SubmitVerificationDocumentResponseDto>> Handle(SubmitVerificationDocumentsCommand request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid)
                return ApiResponse<SubmitVerificationDocumentResponseDto>
                    .Fail(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), 400);

            var user = await _context.Users
                .Include(u => u.VerificationDocuments)
                    .ThenInclude(d => d.StoredFile)
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

            var uploadTasks = request.Documents.Select(async document =>
            {
                if (document is null)
                {
                    return new DocumentProcessingResult(Success: false, Error: new DocumentUploadErrorDto { FileName = string.Empty, Error = "Document is null.", Type = VerificationDocumentType.Other });
                }

                if (document.File.Length == 0)
                {
                    return new DocumentProcessingResult(Success: false, Error: new DocumentUploadErrorDto { FileName = document.File.FileName, Error = "Document is empty.", Type = document.Type });
                }

                if (document.File.Length > 5 * 1024 * 1024)
                {
                    return new DocumentProcessingResult(Success: false, Error: new DocumentUploadErrorDto { FileName = document.File.FileName, Error = "Document size exceeds the maximum allowed limit of 5MB.", Type = document.Type });
                }

                if (!AllowedImageContentTypes.Contains(document.File.ContentType))
                {
                    return new DocumentProcessingResult(Success: false, Error: new DocumentUploadErrorDto { FileName = document.File.FileName, Error = "Only JPEG, PNG, WEBP, HEIC, and HEIF images are allowed.", Type = document.Type });
                }

                var responseItem = new UploadedDocumentDto
                {
                    FileName = document.File.FileName,
                    Type = document.Type
                };

                if (document.ExpirationDate <= DateOnly.FromDateTime(DateTime.Today))
                {
                    return new DocumentProcessingResult(Success: false, Error: new DocumentUploadErrorDto { FileName = document.File.FileName, Error = "This document is expired", Type = document.Type });
                }

                try
                {
                    string folder = document.Type switch
                    {
                        VerificationDocumentType.NationalIdFront or VerificationDocumentType.NationalIdBack => "national-id",
                        VerificationDocumentType.BarAssociationCardFront or VerificationDocumentType.BarAssociationCardBack => "bar-membership",
                        VerificationDocumentType.SelfieWithId => "selfie",
                        VerificationDocumentType.OfficialProfilePicture => "profile-picture",
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

                    StoredFile file = new()
                    {
                        StoredFileName = fileName,
                        OriginalFileName = document.File.FileName,
                        ContentType = document.File.ContentType,
                        Extension = Path.GetExtension(document.File.FileName),
                        SizeInBytes = document.File.Length,
                        FileUrl = uploadResult.StoragePath,
                    };

                    UserVerificationDocument verificationDocument = new()
                    {
                        DocumentType = document.Type,
                        IsCurrent = true,
                        StoredFile = file,
                        User = user,
                        ExpirationDate = document.ExpirationDate,
                        Status = VerificationDocumentStatus.Pending
                    };

                    return new DocumentProcessingResult(Success: true, UploadResult: uploadResult, File: file, VerificationDocument: verificationDocument, ResponseItem: responseItem);
                }
                catch (Exception ex)
                {
                    return new DocumentProcessingResult(Success: false, Error: new DocumentUploadErrorDto { FileName = document.File.FileName, Error = $"An error occurred while uploading the document: {ex.Message}", Type = document.Type });
                }
            }).ToList();

            var results = await Task.WhenAll(uploadTasks);

            foreach (var result in results)
            {
                if (!result.Success && result.Error != null)
                {
                    responseDto.FailedDocuments.Add(result.Error);
                }
                else if (result.Success && result.UploadResult != null && result.ResponseItem != null && result.File != null && result.VerificationDocument != null)
                {
                    uploadedPaths.Add(result.UploadResult.StoragePath);
                    responseDto.UploadedDocuments.Add(result.ResponseItem);
                    _context.StoredFiles.Add(result.File);

                    foreach (var previousVersion in user.VerificationDocuments.Where(d =>
                                 d.DocumentType == result.VerificationDocument.DocumentType &&
                                 d.IsCurrent))
                    {
                        if (previousVersion.Status == VerificationDocumentStatus.Rejected)
                        {
                            // If it was rejected, we don't need to keep the old file in storage or DB
                            if (previousVersion.StoredFile != null)
                            {
                                await _fileStorageService.DeleteAsync(previousVersion.StoredFile.FileUrl, cancellationToken);
                                _context.StoredFiles.Remove(previousVersion.StoredFile);
                            }
                            _context.UserVerificationDocuments.Remove(previousVersion);
                        }
                        else
                        {
                            previousVersion.IsCurrent = false; // Mark old approved ones as not current until admin approves the new one
                        }
                    }
                    _context.UserVerificationDocuments.Add(result.VerificationDocument);
                }
            }

            try
            {
                if (responseDto.UploadedDocuments.Count > 0)
                {
                    user.Status = UserStatus.PendingReview;
                    _context.Users.Update(user);

                    await VerificationOutbox.EnqueueReviewRequestedAsync(
                        _outboxWriter,
                        user,
                        responseDto.UploadedDocuments.Count,
                        Guid.NewGuid(),
                        cancellationToken);
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
