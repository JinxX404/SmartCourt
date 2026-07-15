using SmartCourt.Features.Auth;
using SmartCourt.Features.Auth.Login;
using SmartCourt.Features.Auth.RegisterClient;
using SmartCourt.Features.Auth.RegisterLawyer;

namespace SmartCourt.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterClientAsync(RegisterClientRequest request);
    Task<RegisterResponse> RegisterLawyerAsync(RegisterLawyerRequest request);
    Task ConfirmEmailAsync(string userId, string token);
}
