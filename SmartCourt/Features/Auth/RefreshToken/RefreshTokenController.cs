using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.Login.DTOs;
using SmartCourt.Features.Auth.RefreshToken.DTOs;

namespace SmartCourt.Features.Auth.RefreshToken;

[ApiController]
[Route("api/auth/refresh")]
public class RefreshTokenController(IRefreshTokenService refreshTokenService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await refreshTokenService.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);
        return Ok(ApiResponse<RefreshTokenResponse>.Ok(response));
    }
}
