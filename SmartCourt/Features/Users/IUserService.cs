using SmartCourt.Features.Users.Profile;

namespace SmartCourt.Features.Users;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(string userId);
}
