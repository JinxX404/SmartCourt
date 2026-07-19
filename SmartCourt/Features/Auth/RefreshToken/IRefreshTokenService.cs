using SmartCourt.Features.Auth.Login.DTOs;
using SmartCourt.Common.Entities;
using SmartCourt.Features.Auth.Login;
using SmartCourt.Features.Auth.RefreshToken.DTOs;

namespace SmartCourt.Features.Auth.RefreshToken;

public interface IRefreshTokenService
{
    Task<RefreshTokenResponse> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
