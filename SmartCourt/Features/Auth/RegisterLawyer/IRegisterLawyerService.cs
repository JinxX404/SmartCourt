using SmartCourt.Features.Auth.RegisterLawyer.DTOs;
using SmartCourt.Features.Auth.RegisterClient.DTOs;
namespace SmartCourt.Features.Auth.RegisterLawyer;

public interface IRegisterLawyerService
{
    Task<RegisterResponse> RegisterLawyerAsync(RegisterLawyerRequest request, CancellationToken cancellationToken = default);
}
