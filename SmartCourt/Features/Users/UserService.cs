using SmartCourt.Features.Users.Profile;

namespace SmartCourt.Features.Users;

public class UserService : IUserService
{
    public Task<UserProfileResponse> GetProfileAsync(string userId)
    {
        /*
         * ALGORITHM:
         * 1. Find user in the database by userId, including necessary related entities (ClientProfile or LawyerProfile).
         * 2. If user doesn't exist, throw NotFoundException.
         * 3. Determine the user's role (either from ASP.NET Identity roles or checking which profile object is not null).
         * 4. Map the User properties to UserProfileResponse (Id, Email, FirstName, LastName, PhoneNumber, Role, etc.).
         * 5. If Role == "Client", map the ClientProfile properties to ClientProfileDto.
         * 6. If Role == "Lawyer", map the LawyerProfile properties to LawyerProfileDto (including specializations).
         * 7. Return the constructed UserProfileResponse object.
         */
        throw new NotImplementedException();
    }
}
