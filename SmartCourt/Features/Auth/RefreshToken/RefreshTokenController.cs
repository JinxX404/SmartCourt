using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.RefreshToken.DTOs;
using SmartCourt.Common.Entities;
using SmartCourt.Features.Auth.Login.DTOs;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Features.Auth.Login;

namespace SmartCourt.Features.Auth.RefreshToken;

[ApiController]
[Route("api/auth/refresh")]
public class RefreshTokenController(IRefreshTokenService refreshTokenService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await refreshTokenService.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
        return Ok(ApiResponse<LoginResponse>.Ok(response));
    }
}
