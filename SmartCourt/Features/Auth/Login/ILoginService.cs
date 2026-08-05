using SmartCourt.Features.Auth.Login.DTOs;
namespace SmartCourt.Features.Auth.Login;

public interface ILoginService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
