using SmartCourt.Features.Auth.Login.DTOs;
using SmartCourt.Common.Extensions;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Auth.Login;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Features.Auth.RefreshToken;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtProvider _jwtProvider;
    private readonly IAuthHelperService _authHelper;
    private readonly int _refreshTokenExpiryDays = 14;

    public RefreshTokenService(
        UserManager<ApplicationUser> userManager,
        IJwtProvider jwtProvider,
        IAuthHelperService authHelper)
    {
        _userManager = userManager;
        _jwtProvider = jwtProvider;
        _authHelper = authHelper;
    }

    public async Task<LoginResponse> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userIdString = _jwtProvider.ValidateToken(token, validateLifetime: false);
        if (userIdString is null || !Guid.TryParse(userIdString, out var userId))
        {
            throw new BusinessException("Invalid access token.");
        }

        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || !user.IsAccessEligible())
        {
            throw new BusinessException("Invalid refresh token.");
        }

        var userRefreshToken = user.RefreshTokens.SingleOrDefault(rt => rt.Token == refreshToken);
        if (userRefreshToken is null)
        {
            throw new BusinessException("Invalid refresh token.");
        }

        if (!userRefreshToken.IsActive)
        {
            var activeTokens = user.RefreshTokens.Where(t => t.IsActive).ToList();
            foreach (var activeToken in activeTokens)
            {
                activeToken.RevokedOn = DateTime.UtcNow;
            }
            var revokeResult = await _userManager.UpdateAsync(user);
            EnsureSucceeded(revokeResult);
            throw new BusinessException("Invalid or expired refresh token.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtProvider.GenerateToken(user, roles);
        var newRefreshToken = _authHelper.GenerateRefreshToken();

        userRefreshToken.RevokedOn = DateTime.UtcNow;
        user.RefreshTokens.Add(new SmartCourt.Common.Entities.RefreshToken
        {
            Token = newRefreshToken,
            ExpiresOn = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            CreatedOn = DateTime.UtcNow
        });

        var updateResult = await _userManager.UpdateAsync(user);
        EnsureSucceeded(updateResult);

        return new LoginResponse(
             user.Id.ToString(),
             user.Email ?? string.Empty,
             user.FullName,
             roles.FirstOrDefault() ?? "User",
             newAccessToken.Token,
             newAccessToken.ExpiresInSeconds,
             newRefreshToken,
             DateTime.UtcNow.AddDays(_refreshTokenExpiryDays));
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new BusinessException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }
    }
}
