using SmartCourt.Features.Auth.Login.DTOs;
using SmartCourt.Features.Auth.RefreshToken.DTOs;
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
    private readonly int _refreshTokenExpiryDays = 7;

    public RefreshTokenService(
        UserManager<ApplicationUser> userManager,
        IJwtProvider jwtProvider,
        IAuthHelperService authHelper)
    {
        _userManager = userManager;
        _jwtProvider = jwtProvider;
        _authHelper = authHelper;
    }

    public async Task<RefreshTokenResponse> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hashedRefreshToken = _authHelper.HashRefreshToken(refreshToken);

        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.HashedToken == hashedRefreshToken), cancellationToken);

        if (user is null || !user.IsAccessEligible())
        {
            throw new AuthenticationException("رمز التحديث غير صالح أو منتهي الصلاحية.");
        }

        var userRefreshToken = user.RefreshTokens.SingleOrDefault(rt => rt.HashedToken == hashedRefreshToken);
        if (userRefreshToken is null)
        {
            throw new AuthenticationException("رمز التحديث غير صالح أو منتهي الصلاحية.");
        }

        if (!userRefreshToken.IsActive)
        {
            var activeTokens = user.RefreshTokens.Where(t => t.IsActive).ToList();
            foreach (var activeToken in activeTokens)
            {
                activeToken.RevokedOn = DateTimeOffset.UtcNow;
            }
            var revokeResult = await _userManager.UpdateAsync(user);
            EnsureSucceeded(revokeResult);
            throw new AuthenticationException("رمز التحديث غير صالح أو منتهي الصلاحية.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtProvider.GenerateToken(user, roles);
        var newRefreshToken = _authHelper.GenerateRefreshToken();
        var newHashedRefreshToken = _authHelper.HashRefreshToken(newRefreshToken);
        
        userRefreshToken.RevokedOn = DateTimeOffset.UtcNow;
        user.RefreshTokens.Add(new SmartCourt.Common.Entities.RefreshToken
        {
            HashedToken = newHashedRefreshToken,
            ExpiresOn = DateTimeOffset.UtcNow.AddDays(_refreshTokenExpiryDays),
            CreatedOn = DateTimeOffset.UtcNow
        });

        var updateResult = await _userManager.UpdateAsync(user);
        EnsureSucceeded(updateResult);

        return new RefreshTokenResponse(
             newAccessToken.Token,
             newAccessToken.ExpiresInSeconds,
             newRefreshToken,
             DateTimeOffset.UtcNow.AddDays(_refreshTokenExpiryDays));
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new BusinessException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }
    }
}
