using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.Login.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        return Ok(ApiResponse<LoginResponse>.Ok(response));
    }
}
