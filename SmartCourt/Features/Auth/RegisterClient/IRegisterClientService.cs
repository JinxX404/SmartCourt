using SmartCourt.Features.Auth.RegisterClient.DTOs;
namespace SmartCourt.Features.Auth.RegisterClient;

public interface IRegisterClientService
{
    Task<RegisterResponse> RegisterClientAsync(RegisterClientRequest request, CancellationToken cancellationToken = default);
}
