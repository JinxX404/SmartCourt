using SmartCourt.Features.Users.Clients.DTOs;

namespace SmartCourt.Features.Users.Clients;

public interface IClientService
{
    Task<ClientProfileResponse> GetProfileAsync(Guid id);
    Task UpdateProfileAsync(Guid id, UpdateClientProfileRequest request);
    Task DeleteProfileAsync(Guid id);
}
