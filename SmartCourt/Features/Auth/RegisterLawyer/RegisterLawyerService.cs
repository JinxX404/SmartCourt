using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.RegisterClient.DTOs;
using SmartCourt.Features.Auth.RegisterLawyer.DTOs;
using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.Shared;

namespace SmartCourt.Features.Auth.RegisterLawyer;

public class RegisterLawyerService : IRegisterLawyerService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthHelperService _authHelper;

    public RegisterLawyerService(
        UserManager<ApplicationUser> userManager,
        IAuthHelperService authHelper)
    {
        _userManager = userManager;
        _authHelper = authHelper;
    }

    public async Task<RegisterResponse> RegisterLawyerAsync(RegisterLawyerRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new ValidationException("ConfirmPassword", "كلمة المرور وتأكيد كلمة المرور غير متطابقتين.");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ConflictException("البريد الإلكتروني مسجل بالفعل.");
        }

        await _authHelper.EnsureRoleExistsAsync("Lawyer");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            Status = UserStatus.Unverified,
            LawyerProfile = new LawyerProfile
            {
                IsAvailable = true
            }
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new KeyValuePair<string, string[]>(e.Code, new[] { e.Description })));
        }

        await _userManager.AddToRoleAsync(user, "Lawyer");
        await _authHelper.SendConfirmationEmailAsync(user, cancellationToken);

        return new RegisterResponse(user.Id.ToString(), user.Email!, user.FullName, "Lawyer");
    }
}
