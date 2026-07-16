using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common;

namespace SmartCourt.Features.Auth.RevokeRefreshToken;

[ApiController]
[Route("api/auth/revoke")]
public class RevokeRefreshTokenController(IRevokeRefreshTokenService revokeRefreshTokenService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> Revoke([FromBody] RevokeRefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await revokeRefreshTokenService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "تم إبطال رمز التحديث بنجاح."));
    }
}
