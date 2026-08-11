using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Persistence;
using System.Security.Claims;

namespace SmartCourt.Features.Case.DeleteCase;

public class DeleteCaseHandler : IRequestHandler<DeleteCaseCommand, ApiResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeleteCaseHandler(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _httpContextAccessor = contextAccessor;
    }

    public async Task<ApiResponse> Handle(DeleteCaseCommand request, CancellationToken cancellationToken)
    {
        if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            return ApiResponse.Fail(new List<string>{"User is not authenticated"});

        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var clientId = Guid.Parse(userId);

        var existing = await _context.Cases
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (existing == null)
            return ApiResponse.Fail(["Case not found"], 404);

        if (existing.ClientId != clientId)
            return ApiResponse.Fail(["Not authorized to delete this case"], 403);

        existing.IsDeleted = true;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return ApiResponse.Fail(new List<string>{"An error occurred while deleting the case."});
        }

        return ApiResponse.Ok();
    }
}
