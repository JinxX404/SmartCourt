using SmartCourt.Features.Users.Lawyers.DTOs;

namespace SmartCourt.Features.Users.Lawyers;

public interface ILawyerService
{
    Task<LawyerProfileResponse> GetProfileAsync(Guid id);
    Task UpdateProfileAsync(Guid id, UpdateLawyerProfileRequest request);
    Task DeleteProfileAsync(Guid id);
}
