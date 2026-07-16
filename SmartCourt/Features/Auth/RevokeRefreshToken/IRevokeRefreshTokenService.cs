namespace SmartCourt.Features.Auth.RevokeRefreshToken;

public interface IRevokeRefreshTokenService
{
    Task<bool> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
}
