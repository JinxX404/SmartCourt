using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.RevokeRefreshToken.DTOs;
using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Http;

namespace SmartCourt.Features.Auth.RevokeRefreshToken;

[ApiController]
[Route("api/auth/revoke")]
public class RevokeRefreshTokenController(IRevokeRefreshTokenService revokeRefreshTokenService) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> Revoke([FromBody] RevokeRefreshTokenRequest? request, CancellationToken cancellationToken)
    {
        var token = request?.Token ?? string.Empty;
        var refreshToken = Request.Cookies["refreshToken"] ?? request?.RefreshToken ?? string.Empty;
        
        var result = await revokeRefreshTokenService.RevokeRefreshTokenAsync(token, refreshToken, cancellationToken);
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
        
        return Ok(ApiResponse<bool>.Ok(result, "تم إبطال رمز التحديث بنجاح."));
    }
}
