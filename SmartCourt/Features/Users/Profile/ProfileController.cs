using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common;
using System.Security.Claims;

namespace SmartCourt.Features.Users.Profile;

[ApiController]
[Route("api/users/profile")]
[Authorize]
public class ProfileController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        /*
         * ALGORITHM:
         * 1. Extract the UserId from the claims: User.FindFirstValue(ClaimTypes.NameIdentifier).
         * 2. Call IUserService.GetProfileAsync(userId) to fetch the user profile DTO.
         * 3. Return ApiResponse<UserProfileResponse>.Ok(profileDto).
         */
        throw new NotImplementedException();
    }
}
