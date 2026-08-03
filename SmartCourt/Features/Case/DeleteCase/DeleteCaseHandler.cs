using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Persistence;
using System.Security.Claims;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Features.Case.DeleteCase;

public class DeleteCaseHandler : IRequestHandler<DeleteCaseCommand, ApiResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IFileStorageService _fileStorageService;

    public DeleteCaseHandler(ApplicationDbContext context, IHttpContextAccessor contextAccessor, IFileStorageService fileStorageService)
    {
        _context = context;
        _httpContextAccessor = contextAccessor;
        _fileStorageService = fileStorageService;
    }

    public async Task<ApiResponse> Handle(DeleteCaseCommand request, CancellationToken cancellationToken)
    {
        if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            return ApiResponse.Fail(new List<string>{"User is not authenticated"});

        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var clientId = Guid.Parse(userId);

        var existing = await _context.Cases.FindAsync(request.CaseId, cancellationToken);

        if (existing == null)
            return ApiResponse.Fail(["Case not found"], 404);

        if (existing.ClientId != clientId)
            return ApiResponse.Fail(["Not authorized to delete this case"], 403);

        var fileUrls = _context.CaseDocuments
            .Where(cd => cd.CaseId == existing.Id)
            .Select(cd => cd.StoredFile.FileUrl)
            .ToList();

        _context.Cases.Remove(existing);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var url in fileUrls)
            {
                try
                {
                    await _fileStorageService.DeleteAsync(url, cancellationToken);
                }
                catch 
                { }
            }
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail(new List<string>{"An error occurred while deleting the case."});
        }

        return ApiResponse.Ok();
    }
}
