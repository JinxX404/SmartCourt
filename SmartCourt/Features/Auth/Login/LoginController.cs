using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.Login.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Http;

namespace SmartCourt.Features.Auth.Login;

[ApiController]
[Route("api/auth/login")]
public class LoginController(ILoginService loginService) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Post(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await loginService.LoginAsync(request, cancellationToken);
        SetAccessTokenCookie(response.AccessToken, response.ExpiresIn);
        SetRefreshTokenCookie(response.RefreshToken, response.RefreshTokenExpiration);
        return Ok(ApiResponse<LoginResponse>.Ok(response));
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
