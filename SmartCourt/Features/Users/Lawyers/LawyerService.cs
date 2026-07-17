using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Users.Lawyers.DTOs;

namespace SmartCourt.Features.Users.Lawyers;

public class LawyerService(UserManager<ApplicationUser> userManager) : ILawyerService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<LawyerProfileResponse> GetProfileAsync(Guid id)
    {
        // 1. Verify user exists and is a lawyer before fetching profile
        var userExists = await _userManager.FindByIdAsync(id.ToString());
        if (userExists == null)
            throw new NotFoundException("المحامي غير موجود");

        if (!await _userManager.IsInRoleAsync(userExists, "Lawyer"))
            throw new NotFoundException("المستخدم ليس محامياً");

        // 2. Use Projection (.Select) for maximum performance.
        // This avoids tracking overhead and only fetches the EXACT columns needed, skipping .Include()
        var response = await _userManager.Users
            .Where(u => u.Id == id)
            .Select(u => new LawyerProfileResponse
            {
                Id = u.Id,
                Name = u.FullName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                NationalNumber = u.NationalNumber ?? string.Empty,
                Gender = u.Gender ?? string.Empty,
                DateOfBirth = u.DateOfBirth.GetValueOrDefault(),
                Address = u.Address,
                Status = u.Status.ToString(),
                Specialization = u.LawyerProfile != null ? u.LawyerProfile.Specialization : string.Empty,
                YearsOfExperience = u.LawyerProfile != null ? u.LawyerProfile.YearsOfExperience : 0,
                Bio = u.LawyerProfile != null ? u.LawyerProfile.Bio : null
            })
            .FirstOrDefaultAsync();

        return response!;
    }

    public async Task UpdateProfileAsync(Guid id, UpdateLawyerProfileRequest request)
    {
        var user = await _userManager.Users
            .Include(u => u.LawyerProfile)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new NotFoundException("المحامي غير موجود");
        }

        if (!await _userManager.IsInRoleAsync(user, "Lawyer"))
        {
            throw new NotFoundException("المستخدم ليس محامياً");
        }

        if (user.Email != request.Email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, request.Email);
            if (!setEmailResult.Succeeded)
                throw new BusinessException(string.Join(" ", setEmailResult.Errors.Select(e => e.Description)));

            var setUserNameResult = await _userManager.SetUserNameAsync(user, request.Email);
             if (!setUserNameResult.Succeeded)
                throw new BusinessException(string.Join(" ", setUserNameResult.Errors.Select(e => e.Description)));
        }

        if (user.PhoneNumber != request.PhoneNumber)
        {
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, request.PhoneNumber);
            if (!setPhoneResult.Succeeded)
                throw new BusinessException(string.Join(" ", setPhoneResult.Errors.Select(e => e.Description)));
        }

        user.DateOfBirth = request.DateOfBirth;
        user.Address = request.Address;
        
        if (user.LawyerProfile == null)
        {
            user.LawyerProfile = new LawyerProfile { UserId = user.Id };
        }

        user.LawyerProfile.Specialization = request.Specialization;
        user.LawyerProfile.YearsOfExperience = request.YearsOfExperience;
        user.LawyerProfile.Bio = request.Bio;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new BusinessException(string.Join(" ", updateResult.Errors.Select(e => e.Description)));
        }
    }

    public async Task DeleteProfileAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            throw new NotFoundException("المحامي غير موجود");
        }

        if (!await _userManager.IsInRoleAsync(user, "Lawyer"))
        {
            throw new NotFoundException("المستخدم ليس محامياً");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new BusinessException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
