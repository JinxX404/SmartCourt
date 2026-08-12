using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using SmartCourt.Features.Case.AddCaseDocument.DTOs;
using SmartCourt.Features.Case.CreateCase.DTOs;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCourt.Features.Case.AddCaseDocument;

public class AddCaseDocumentService : IAddCaseDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IValidator<AddCaseDocumentRequest> _validator;
    private readonly IFileStorageService _fileStorageService;

    public AddCaseDocumentService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IValidator<AddCaseDocumentRequest> validator,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _validator = validator;
        _fileStorageService = fileStorageService;
    }

    public async Task<ApiResponse<AddCaseDocumentResponse>> AddDocumentsAsync(
        Guid caseId,
        AddCaseDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ApiResponse<AddCaseDocumentResponse>.Fail(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList(), 400);
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return ApiResponse<AddCaseDocumentResponse>.Fail(new List<string> { "User is not authenticated" }, 401);
        }

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<AddCaseDocumentResponse>.Fail(new List<string> { "Invalid user identifier" }, 401);
        }

        var existingCase = await _context.Cases
            .FirstOrDefaultAsync(c => c.Id == caseId, cancellationToken);

        if (existingCase == null)
        {
            return ApiResponse<AddCaseDocumentResponse>.Fail(new List<string> { "Case not found" }, 404);
        }

        var isAdmin = httpContext.User.IsInRole("Admin");
        var isOwner = existingCase.ClientId == userId;
        var isAssignedLawyer = existingCase.LawyerId == userId;

        if (!isOwner && !isAssignedLawyer && !isAdmin)
        {
            return ApiResponse<AddCaseDocumentResponse>.Fail(
                new List<string> { "Not authorized to attach documents to this case" }, 403);
        }

        var uploadedPaths = new List<string>();
        var addedDocuments = new List<AddedDocumentDto>();
        var failedDocuments = new List<CaseDocumentUploadErrorDto>();

        foreach (var document in request.Documents)
        {
            try
            {
                string folder = "case-documents";
                string extension = Path.GetExtension(document.FileName);
                string fileName = $"{Guid.NewGuid()}{extension}";
                string filePath = $"{userId}/{folder}/{fileName}";

                await using var stream = document.OpenReadStream();

                var uploadResult = await _fileStorageService.UploadAsync(
                    stream,
                    filePath,
                    document.FileName,
                    document.ContentType,
                    cancellationToken);

                uploadedPaths.Add(uploadResult.StoragePath);

                StoredFile file = new()
                {
                    Id = Guid.NewGuid(),
                    StoredFileName = fileName,
                    OriginalFileName = document.FileName,
                    ContentType = document.ContentType,
                    Extension = extension,
                    SizeInBytes = document.Length,
                    FileUrl = uploadResult.StoragePath,
                };

                _context.StoredFiles.Add(file);

                CaseDocument caseDocument = new()
                {
                    Id = Guid.NewGuid(),
                    Case = existingCase,
                    StoredFile = file
                };

                _context.CaseDocuments.Add(caseDocument);

                addedDocuments.Add(new AddedDocumentDto
                {
                    DocumentId = caseDocument.Id,
                    StoredFileId = file.Id,
                    FileName = document.FileName,
                    FileUrl = uploadResult.StoragePath,
                    SizeInBytes = document.Length
                });
            }
            catch (Exception ex)
            {
                failedDocuments.Add(new CaseDocumentUploadErrorDto
                {
                    FileName = document.FileName,
                    Error = $"Error while uploading document: {ex.Message}"
                });
            }
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            foreach (var filePath in uploadedPaths)
            {
                await _fileStorageService.DeleteAsync(filePath, cancellationToken);
            }

            return ApiResponse<AddCaseDocumentResponse>.Fail(
                new List<string> { "An error occurred while saving the uploaded documents." }, 500);
        }

        return ApiResponse<AddCaseDocumentResponse>.Ok(new AddCaseDocumentResponse
        {
            CaseId = caseId,
            AddedDocuments = addedDocuments,
            FailedDocuments = failedDocuments
        });
    }
}
