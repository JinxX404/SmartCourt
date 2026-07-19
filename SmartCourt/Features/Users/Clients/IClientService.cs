using SmartCourt.Features.Users.Clients.DTOs;
using SmartCourt.Features.Users.Shared.DTOs;

namespace SmartCourt.Features.Users.Clients;

public interface IClientService
{
    Task<ClientProfileResponse> GetProfileAsync(CancellationToken cancellationToken);
    Task UpdateProfileAsync(UpdateClientProfileRequest request, CancellationToken cancellationToken);
    Task DeleteProfileAsync(DeleteAccountRequest request, CancellationToken cancellationToken);
}
