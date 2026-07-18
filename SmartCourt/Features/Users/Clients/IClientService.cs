using SmartCourt.Features.Users.Clients.DTOs;

namespace SmartCourt.Features.Users.Clients;

public interface IClientService
{
    Task<ClientProfileResponse> GetProfileAsync(CancellationToken cancellationToken);
    Task UpdateProfileAsync(UpdateClientProfileRequest request, CancellationToken cancellationToken);
    Task DeleteProfileAsync(CancellationToken cancellationToken);
}
