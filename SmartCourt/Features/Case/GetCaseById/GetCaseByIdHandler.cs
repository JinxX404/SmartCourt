using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Persistence;
using SmartCourt.Features.Case.GetCaseById.DTOs;

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

        var dto = new CaseDto
        {
            Id = existing.Id,
            ClientId = existing.ClientId,
            Title = existing.Title,
            Description = existing.Description,
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
