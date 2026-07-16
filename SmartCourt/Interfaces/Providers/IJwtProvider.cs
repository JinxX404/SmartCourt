using SmartCourt.Features.Auth;

namespace SmartCourt.Interfaces.Providers;

public interface IJwtProvider
{
    TokenResult GenerateToken(ApplicationUser user, IEnumerable<string> roles);
    string? ValidateToken(string token, bool validateLifetime = true);
}
