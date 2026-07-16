using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartCourt.Common;
using SmartCourt.Interfaces;
using SmartCourt.Features.Auth;

namespace SmartCourt.Features.Auth.RegisterClient;

[ApiController]
[Route("api/auth/register/client")]
public class RegisterClientController : ControllerBase
{
    private readonly IAuthService _authService;

    public RegisterClientController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Post(RegisterClientRequest request)
    {
        var response = await _authService.RegisterClientAsync(request);
        var apiResponse = ApiResponse<RegisterResponse>.Created(response);
        apiResponse.Message = "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني";
        return Created(string.Empty, apiResponse);
    }
}
