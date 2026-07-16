namespace SmartCourt.Features.Auth.RegisterLawyer;

public interface IRegisterLawyerService
{
    Task<RegisterResponse> RegisterLawyerAsync(RegisterLawyerRequest request, CancellationToken cancellationToken = default);
}
