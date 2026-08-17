using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Persistence;
using SmartCourt.Features.Case.GetCaseById.DTOs;
using SmartCourt.Features.Contracts.Enums;

namespace SmartCourt.Features.Case.GetCaseById;

public class GetCaseByIdHandler : IRequestHandler<GetCaseByIdQuery, ApiResponse<CaseDto>>
{
    private readonly ApplicationDbContext _context;

    public GetCaseByIdHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<CaseDto>> Handle(GetCaseByIdQuery request, CancellationToken cancellationToken)
    {
        var existing = await _context.Cases
            .Include(c => c.Documents)
            .ThenInclude(cd => cd.StoredFile)
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (existing == null)
            return ApiResponse<CaseDto>.Fail(new List<string>{"Case not found"}, 404);

        var lawyerId = existing.LawyerId ?? await _context.Contracts
            .Where(ct => ct.LegalCaseId == existing.Id && (ct.Status == ContractStatus.Active || ct.Status == ContractStatus.CompletedOnHold || ct.Status == ContractStatus.Completed))
            .Select(ct => (Guid?)ct.LawyerUserId)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new CaseDto
        {
            Id = existing.Id,
            ClientId = existing.ClientId,
            LawyerId = lawyerId,
            LastReviewId = existing.LastReviewId,
            ChatId = existing.ChatId,
            Title = existing.Title,
            Description = existing.Description,
            Governorate = existing.Governorate,
            City = existing.City,
            Status = existing.Status.ToString(),
            CreatedAt = existing.CreatedAt,
            Documents = existing.Documents?.Select(cd => new CaseDocumentDto
            {
                Id = cd.StoredFile.Id,
                FileName = cd.StoredFile.OriginalFileName,
                FileUrl = cd.StoredFile.FileUrl,
                ContentType = cd.StoredFile.ContentType
            }).ToList() ?? new List<CaseDocumentDto>()
        };

        return ApiResponse<CaseDto>.Ok(dto);
    }
}
