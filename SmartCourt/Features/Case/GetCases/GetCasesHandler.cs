using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Persistence;
using SmartCourt.Features.Case.GetCases.DTOs;

namespace SmartCourt.Features.Case.GetCases;

public class GetCasesHandler : IRequestHandler<GetCasesQuery, ApiResponse<List<CaseListItemDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetCasesHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<CaseListItemDto>>> Handle(GetCasesQuery request, CancellationToken cancellationToken)
    {
        var cases = await _context.Cases.Select(c => new
        {
            c.Id,
            c.Title,
            c.Status,
            c.CreatedAt,
            DocumentCount = c.Documents.Count(),
        }).ToListAsync(cancellationToken);

        var result = cases.Select(c => new CaseListItemDto
        {
            Id = c.Id,
            Title = c.Title,
            Status = c.Status.ToString(),
            CreatedAt = c.CreatedAt,
            DocumentCount = c.DocumentCount
        }).ToList();

        return ApiResponse<List<CaseListItemDto>>.Ok(result);
    }
}
