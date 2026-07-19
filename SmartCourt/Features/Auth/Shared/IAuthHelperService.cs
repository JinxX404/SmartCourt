namespace SmartCourt.Features.Auth.Shared;

public interface IAuthHelperService
{
    Task EnsureRoleExistsAsync(string roleName);
    Task SendConfirmationEmailAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task SendChangeEmailConfirmationAsync(ApplicationUser user, string newEmail, CancellationToken cancellationToken = default);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
    void RevokeAllActiveRefreshTokens(ApplicationUser applicationUser);
}
