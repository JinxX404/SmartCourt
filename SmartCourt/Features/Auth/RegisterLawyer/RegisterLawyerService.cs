using Microsoft.AspNetCore.Identity;
using SmartCourt.Common;
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

        await _authHelper.EnsureRoleExistsAsync("Lawyer");

        var fullAddress = string.Join(", ", new[] { request.Address.Trim(), request.City.Trim(), request.Government.Trim() }.Where(x => !string.IsNullOrWhiteSpace(x)));

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.Phone,
            Address = fullAddress,
            Gender = request.Gender,
            NationalNumber = request.NationalNumber,
            Status = UserStatus.Unverified
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
