using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.Login.DTOs;
using SmartCourt.Features.Auth.RefreshToken.DTOs;

using Microsoft.AspNetCore.Http;

namespace SmartCourt.Features.Auth.RefreshToken;

[ApiController]
[Route("api/auth/refresh")]
public class RefreshTokenController(IRefreshTokenService refreshTokenService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> Refresh([FromBody] RefreshTokenRequest? request, CancellationToken cancellationToken)
    {
        var token = Request.Cookies["refreshToken"] ?? request?.RefreshToken;
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized(ApiResponse<object>.Fail("Missing refresh token", 401));
        }

        var response = await refreshTokenService.GetRefreshTokenAsync(token, cancellationToken);
        SetAccessTokenCookie(response.AccessToken, response.AccessTokenExpiresInSeconds);
        SetRefreshTokenCookie(response.RefreshToken, response.ExpiresAt);
        return Ok(ApiResponse<RefreshTokenResponse>.Ok(response));
    }

    private void SetAccessTokenCookie(string token, int expiresInSeconds)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds),
            Secure = false,
            SameSite = SameSiteMode.Lax
        };
        Response.Cookies.Append("accessToken", token, cookieOptions);
    }

    private void SetRefreshTokenCookie(string token, DateTimeOffset expires)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = expires,
            Secure = false,
            SameSite = SameSiteMode.Lax
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }
}
