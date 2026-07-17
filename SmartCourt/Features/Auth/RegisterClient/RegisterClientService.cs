using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.RegisterClient.DTOs;
using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.Shared;

namespace SmartCourt.Features.Auth.RegisterClient;

public class RegisterClientService : IRegisterClientService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthHelperService _authHelper;

    public RegisterClientService(
        UserManager<ApplicationUser> userManager,
        IAuthHelperService authHelper)
    {
        _userManager = userManager;
        _authHelper = authHelper;
    }

    public async Task<RegisterResponse> RegisterClientAsync(RegisterClientRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new ValidationException("ConfirmPassword", "كلمة المرور وتأكيد كلمة المرور غير متطابقتين.");
        }

        await _authHelper.EnsureRoleExistsAsync("Client");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            NationalNumber = request.NationalNumber,
            Status = UserStatus.Unverified
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new KeyValuePair<string, string[]>(e.Code, new[] { e.Description })));
        }

        await _userManager.AddToRoleAsync(user, "Client");
        await _authHelper.SendConfirmationEmailAsync(user, cancellationToken);

        return new RegisterResponse(user.Id.ToString(), user.Email!, user.FullName, "Client");
    }
}
