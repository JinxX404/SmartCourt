using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Domain;
using SmartCourt.Common.Models;
using SmartCourt.Features.Case.GetUserCases.DTOs;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Case.GetUserCases;

public sealed class GetUserCasesService : IGetUserCasesService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserCasesService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<GetUserCaseSummaryDto>> GetUserCasesAsync(
        GetUserCasesQuery query,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.RequireUserId("المستخدم غير معروف أو غير مسجل الدخول.");

        var casesQuery = _context.Cases
            .AsNoTracking()
            .Where(c => c.ClientId == currentUserId);

        if (query.Status.HasValue)
        {
            casesQuery = casesQuery.Where(c => c.Status == query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            casesQuery = casesQuery.Where(c => c.Title.Contains(term));
        }

        var totalCount = await casesQuery.CountAsync(cancellationToken);

        var items = await casesQuery
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new GetUserCaseSummaryDto(
                c.Id,
                c.Title,
                c.Status.ToString(),
                c.Governorate,
                c.City,
                c.SubmittedAt,
                c.CreatedAt,
                c.Documents.Count))
            .ToListAsync(cancellationToken);

        var hasNextPage = (query.Page * query.PageSize) < totalCount;

        return new PagedResult<GetUserCaseSummaryDto>(
            items,
            query.Page,
            query.PageSize,
            totalCount,
            hasNextPage);
    }
}
