using SmartCourt.Common.Entities;
namespace SmartCourt.Features.Auth.RevokeRefreshToken.DTOs;

public record RevokeRefreshTokenRequest(
    string? Token = null,
    string? RefreshToken = null
);
