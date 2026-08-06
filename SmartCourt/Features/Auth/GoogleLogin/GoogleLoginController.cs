using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.GoogleLogin.DTOs;
using SmartCourt.Features.Auth.Login.DTOs;

namespace SmartCourt.Features.Auth.GoogleLogin;

[ApiController]
[Route("api/auth")]
public class GoogleLoginController : ControllerBase
{
    private readonly IGoogleLoginService _googleLoginService;

    public GoogleLoginController(IGoogleLoginService googleLoginService)
    {
        _googleLoginService = googleLoginService;
    }

    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> GoogleLogin(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _googleLoginService.LoginWithGoogleAsync(request, cancellationToken);
        
        SetAccessTokenCookie(result.AccessToken, result.ExpiresIn);
        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return Ok(ApiResponse<LoginResponse>.Ok(result, "تم تسجيل الدخول بواسطة جوجل بنجاح"));
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

    private void SetRefreshTokenCookie(string refreshToken, DateTime expires)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = expires,
            Secure = false,
            SameSite = SameSiteMode.Lax
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
