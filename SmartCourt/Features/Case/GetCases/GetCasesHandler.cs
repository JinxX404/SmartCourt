using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Domain;
using SmartCourt.Common.Models;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using SmartCourt.Features.Case.GetCases.DTOs;

namespace SmartCourt.Features.Case.GetCases;

public class GetCasesHandler : IRequestHandler<GetCasesQuery, ApiResponse<List<CaseListItemDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCasesHandler(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<List<CaseListItemDto>>> Handle(GetCasesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId("المستخدم غير معروف");

        var isAdmin = await HasRoleAsync(userId, "Admin", cancellationToken);
        var isClient = await HasRoleAsync(userId, "Client", cancellationToken);
        var isLawyer = await HasRoleAsync(userId, "Lawyer", cancellationToken);

        var query = _context.Cases.AsQueryable();

        if (isAdmin)
        {
            // Admin sees all cases
        }
        else if (isClient)
        {
            query = query.Where(c => c.ClientId == userId);
        }
        else if (isLawyer)
        {
            query = query.Where(c =>
                _context.Proposals.Any(p => p.LegalCaseId == c.Id && p.LawyerUserId == userId)
                || _context.Contracts.Any(ct => ct.LegalCaseId == c.Id && ct.LawyerUserId == userId));
        }
        else
        {
            return ApiResponse<List<CaseListItemDto>>.Ok(new List<CaseListItemDto>());
        }

        var cases = await query.Select(c => new
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

    private async Task<bool> HasRoleAsync(Guid userId, string roleName, CancellationToken ct)
    {
        return await (
            from ur in _context.UserRoles
            join r in _context.Roles on ur.RoleId equals r.Id
            where ur.UserId == userId && r.Name == roleName
            select ur.UserId
        ).AnyAsync(ct);
    }
}
