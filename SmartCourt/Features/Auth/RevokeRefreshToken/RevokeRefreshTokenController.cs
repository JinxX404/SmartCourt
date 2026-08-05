using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.RevokeRefreshToken.DTOs;
using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace SmartCourt.Features.Auth.RevokeRefreshToken;

[ApiController]
[Route("api/auth/revoke")]
public class RevokeRefreshTokenController(IRevokeRefreshTokenService revokeRefreshTokenService) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> Revoke([FromBody] RevokeRefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await revokeRefreshTokenService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "تم إبطال رمز التحديث بنجاح."));
    }
}
