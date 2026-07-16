using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Features.Auth.RevokeRefreshToken;

public class RevokeRefreshTokenService : IRevokeRefreshTokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtProvider _jwtProvider;

    public RevokeRefreshTokenService(
        UserManager<ApplicationUser> userManager,
        IJwtProvider jwtProvider)
    {
        _userManager = userManager;
        _jwtProvider = jwtProvider;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userIdString = _jwtProvider.ValidateToken(token, validateLifetime: false);
        if (userIdString is null || !Guid.TryParse(userIdString, out var userId))
        {
            throw new BusinessException("Invalid access token.");
        }

        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new BusinessException("Invalid refresh token.");
        }

        var userRefreshToken = user.RefreshTokens.SingleOrDefault(rt => rt.Token == refreshToken);
        if (userRefreshToken is null || !userRefreshToken.IsActive)
        {
            return false;
        }

        userRefreshToken.RevokedOn = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return true;
    }
}
