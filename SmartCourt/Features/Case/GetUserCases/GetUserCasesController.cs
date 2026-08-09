using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Common.Validation;
using SmartCourt.Features.Case.GetUserCases.DTOs;

namespace SmartCourt.Features.Case.GetUserCases;

[ApiController]
[Route("api/cases")]
[Authorize]
public sealed class GetUserCasesController : ControllerBase
{
    private readonly IGetUserCasesService _getUserCasesService;
    private readonly IValidator<GetUserCasesQuery> _validator;

    public GetUserCasesController(
        IGetUserCasesService getUserCasesService,
        IValidator<GetUserCasesQuery> validator)
    {
        _getUserCasesService = getUserCasesService;
        _validator = validator;
    }

    [HttpGet("my")]
    [HttpGet("my-cases")]
    [HttpGet("/api/case/my-cases")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    public async Task<ActionResult<ApiResponse<PagedResult<GetUserCaseSummaryDto>>>> GetMyCasesAsync(
        [FromQuery] GetUserCasesQuery query,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowBusinessExceptionAsync(query, cancellationToken);
        var result = await _getUserCasesService.GetUserCasesAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<GetUserCaseSummaryDto>>.Ok(result));
    }
}
