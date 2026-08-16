using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using SmartCourt.Features.Case.CreateCase.DTOs;
using SmartCourt.Features.UserVerification.SubmitVerificationDocuments.DTOs;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using System.Security.Claims;

namespace SmartCourt.Features.Case.CreateCase
{
    public class CreateCaseHandler : IRequestHandler<CreateCaseCommand, ApiResponse<CreateCaseResponse>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IValidator<CreateCaseCommand> _validator;
        private readonly IFileStorageService _fileStorageService;

        public CreateCaseHandler(ApplicationDbContext context, IHttpContextAccessor contextAccessor,
            IValidator<CreateCaseCommand> validator, IFileStorageService fileStorageService)
        {
            _context = context;
            _httpContextAccessor = contextAccessor;
            _validator = validator;
            _fileStorageService = fileStorageService;
        }

        public async Task<ApiResponse<CreateCaseResponse>> Handle(CreateCaseCommand request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid)
                return ApiResponse<CreateCaseResponse>
                    .Fail(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), 400);

            if (!_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? true)
                return ApiResponse<CreateCaseResponse>.Fail(["User is not authenticated"]);

            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var clientId = Guid.Parse(userId!);

            var clientUser = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == clientId, cancellationToken);

            var uploadedPaths = new List<string>();
            var failedDocuments = new List<CaseDocumentUploadErrorDto>();

            var governorate = !string.IsNullOrWhiteSpace(request.Governorate)
                ? request.Governorate
                : clientUser?.Governorate;

            var city = !string.IsNullOrWhiteSpace(request.City)
                ? request.City
                : clientUser?.City;

            var clientProfileExists = await _context.Set<SmartCourt.Common.Entities.ClientProfile>()
                .AnyAsync(cp => cp.UserId == clientId, cancellationToken);
                
            if (!clientProfileExists)
            {
                return ApiResponse<CreateCaseResponse>.Fail(["Only users registered as Clients can create cases."], 403);
            }

            SmartCourt.Entities.Case legalCase = new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Title = request.Title,
                Description = request.Description,
                Governorate = governorate,
                City = city,
                Status = CaseStatus.Submitted
            };

            _context.Cases.Add(legalCase);

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
                            Case = legalCase,
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

                return ApiResponse<CreateCaseResponse>
                    .Fail(["An error occured while uploading your documents. Try again please.."]);
            }

            return ApiResponse<CreateCaseResponse>
                .Created(new CreateCaseResponse
                {
                    CaseId = legalCase.Id,
                    FailedDocuments = failedDocuments
                });
        }
    }
}
