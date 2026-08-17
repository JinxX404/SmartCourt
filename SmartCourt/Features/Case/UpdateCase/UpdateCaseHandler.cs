using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using SmartCourt.Features.Case.CreateCase.DTOs;
using SmartCourt.Features.Case.UpdateCase.DTOs;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using System.Security.Claims;

namespace SmartCourt.Features.Case.UpdateCase;

public class UpdateCaseHandler : IRequestHandler<UpdateCaseCommand, ApiResponse<UpdateCaseResponse>>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IValidator<UpdateCaseCommand> _validator;
    private readonly IFileStorageService _fileStorageService;

    public UpdateCaseHandler(ApplicationDbContext context, IHttpContextAccessor contextAccessor,
        IValidator<UpdateCaseCommand> validator, IFileStorageService fileStorageService)
    {
        _context = context;
        _httpContextAccessor = contextAccessor;
        _validator = validator;
        _fileStorageService = fileStorageService;
    }

    public async Task<ApiResponse<UpdateCaseResponse>> Handle(UpdateCaseCommand request, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid)
            return ApiResponse<UpdateCaseResponse>.Fail(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), 400);

        if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            return ApiResponse<UpdateCaseResponse>.Fail(new List<string>{"User is not authenticated"});

        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var clientId = Guid.Parse(userId);

        var existing = await _context.Cases
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (existing == null)
            return ApiResponse<UpdateCaseResponse>.Fail(new List<string>{"Case not found"}, 404);

        // Ensure the current user is owner (client) of the case
        if (existing.ClientId != clientId)
            return ApiResponse<UpdateCaseResponse>.Fail(new List<string>{"Not authorized to update this case"}, 403);

        if (existing.Status == SmartCourt.Common.Enums.CaseStatus.Assigned)
            return ApiResponse<UpdateCaseResponse>.Fail(new List<string> { "Cannot update a case that has already been assigned." }, 400);

        var clientUser = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == clientId, cancellationToken);

        existing.Title = request.Title;
        existing.Description = request.Description;

        existing.Governorate = !string.IsNullOrWhiteSpace(request.Governorate)
            ? request.Governorate
            : (!string.IsNullOrWhiteSpace(existing.Governorate) ? existing.Governorate : clientUser?.Governorate);

        existing.City = !string.IsNullOrWhiteSpace(request.City)
            ? request.City
            : (!string.IsNullOrWhiteSpace(existing.City) ? existing.City : clientUser?.City);

        if (existing.Status == SmartCourt.Common.Enums.CaseStatus.Reviewed)
        {
            existing.Status = SmartCourt.Common.Enums.CaseStatus.Submitted;
        }

        var uploadedPaths = new List<string>();
        var failedDocuments = new List<CaseDocumentUploadErrorDto>();

        if (request.Documents != null)
        {
            foreach (var document in request.Documents)
            {
                try
                {
                    string folder = "case-documents";

                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(document.FileName)}";
                    string filePath = $"{clientId}/{folder}/{fileName}";

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
                        StoredFileName = fileName,
                        OriginalFileName = document.FileName,
                        ContentType = document.ContentType,
                        Extension = Path.GetExtension(document.FileName),
                        SizeInBytes = document.Length,
                        FileUrl = uploadResult.StoragePath,
                    };

                    _context.StoredFiles.Add(file);

                    CaseDocument caseDocument = new()
                    {
                        Case = existing,
                        StoredFile = file
                    };

                    _context.CaseDocuments.Add(caseDocument);
                }
                catch (Exception ex)
                {
                    failedDocuments.Add(new CaseDocumentUploadErrorDto
                    {
                        FileName = document.FileName,
                        Error = $"Error while uploading document : {ex.Message}"
                    });
                }
            }
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            foreach (var filePath in uploadedPaths)
            {
                await _fileStorageService.DeleteAsync(filePath, cancellationToken);
            }

            return ApiResponse<UpdateCaseResponse>.Fail(new List<string>{"An error occured while updating the case."});
        }

        return ApiResponse<UpdateCaseResponse>.Ok(new UpdateCaseResponse
        {
            CaseId = existing.Id,
            FailedDocuments = failedDocuments
        });
    }
}
