using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.Login.DTOs;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Features.Auth.Login;

public class LoginService : ILoginService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtProvider _jwtProvider;
    private readonly IAuthHelperService _authHelper;
    private readonly int _refreshTokenExpiryDays = 7;

    public LoginService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtProvider jwtProvider,
        IAuthHelperService authHelper)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtProvider = jwtProvider;
        _authHelper = authHelper;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
        {
            throw new AuthenticationException("البريد الإلكتروني أو كلمة المرور غير صحيحة.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            throw new AuthenticationException("البريد الإلكتروني أو كلمة المرور غير صحيحة.");
        }

        if (!user.EmailConfirmed)
        {
            throw new ForbiddenAccessException("يرجى تأكيد البريد الإلكتروني أولاً");
        }

        if (user.Status == UserStatus.Suspended)
        {
            throw new ForbiddenAccessException("تم تعليق حسابك. تواصل مع الدعم");
        }

        if (user.Status == UserStatus.Deleted)
        {
            throw new AuthenticationException("البريد الإلكتروني أو كلمة المرور غير صحيحة.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var tokenResult = _jwtProvider.GenerateToken(user, roles);

        var refreshToken = _authHelper.GenerateRefreshToken();
        var hashedRefreshToken = _authHelper.HashRefreshToken(refreshToken);
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        user.RefreshTokens.Add(new SmartCourt.Common.Entities.RefreshToken
        {
            HashedToken = hashedRefreshToken,
            ExpiresOn = refreshTokenExpiration
        });

        user.LastLoginAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        var userDto = new UserDto(
            user.Id.ToString(),
            user.Email ?? string.Empty,
            user.FullName,
            roles.FirstOrDefault() ?? "User"
        );

        return new LoginResponse(
            userDto,
            tokenResult.Token,
            tokenResult.ExpiresInSeconds,
            refreshToken,
            refreshTokenExpiration);
    }
}
