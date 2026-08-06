using SmartCourt.Common.Models;
using SmartCourt.Features.Users.Lawyers.DTOs;
using SmartCourt.Features.Users.Shared.DTOs;

namespace SmartCourt.Features.Users.Lawyers;

public interface ILawyerService
{
    Task<LawyerProfileResponse> GetProfileAsync(CancellationToken cancellationToken);
    Task<PublicLawyerProfileResponse> GetPublicProfileAsync(Guid lawyerId, CancellationToken cancellationToken);
    Task<PagedResponse<List<PublicLawyerProfileResponse>>> SearchLawyersAsync(SearchLawyersRequest request, CancellationToken cancellationToken);
    Task CompleteProfileAsync(CompleteLawyerProfileRequest request, CancellationToken cancellationToken);
    Task UpdateProfileAsync(UpdateLawyerProfileRequest request, CancellationToken cancellationToken);
    Task DeleteProfileAsync(DeleteAccountRequest request, CancellationToken cancellationToken);
}
