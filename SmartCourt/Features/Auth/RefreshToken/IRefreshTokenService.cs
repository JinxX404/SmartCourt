using SmartCourt.Features.Auth.Login.DTOs;
using SmartCourt.Common.Entities;
using SmartCourt.Features.Auth.Login;

namespace SmartCourt.Features.Auth.RefreshToken;

public interface IRefreshTokenService
{
    Task<LoginResponse> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
}
