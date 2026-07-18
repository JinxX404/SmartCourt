using SmartCourt.Features.Users.Lawyers.DTOs;

namespace SmartCourt.Features.Users.Lawyers;

public interface ILawyerService
{
    Task<LawyerProfileResponse> GetProfileAsync(CancellationToken cancellationToken);
    Task<PublicLawyerProfileResponse> GetPublicProfileAsync(Guid lawyerId, CancellationToken cancellationToken);
    Task UpdateProfileAsync(UpdateLawyerProfileRequest request, CancellationToken cancellationToken);
    Task DeleteProfileAsync(CancellationToken cancellationToken);
}
