using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Users.Clients.DTOs;

namespace SmartCourt.Features.Users.Clients;

public class ClientService(UserManager<ApplicationUser> userManager) : IClientService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<ClientProfileResponse> GetProfileAsync(Guid id)
    {
        var userExists = await _userManager.FindByIdAsync(id.ToString());
        if (userExists == null)
            throw new NotFoundException("الموكل غير موجود");

        if (!await _userManager.IsInRoleAsync(userExists, "Client"))
            throw new NotFoundException("المستخدم ليس موكلاً");

        var response = await _userManager.Users
            .Where(u => u.Id == id)
            .Select(u => new ClientProfileResponse
            {
                Id = u.Id,
                Name = u.FullName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                NationalNumber = u.NationalNumber ?? string.Empty,
                Gender = u.Gender ?? string.Empty,
                DateOfBirth = u.DateOfBirth.GetValueOrDefault(),
                Address = u.Address,
                Status = u.Status.ToString()
            })
            .FirstOrDefaultAsync();

        return response!;
    }

    public async Task UpdateProfileAsync(Guid id, UpdateClientProfileRequest request)
    {
        var user = await _userManager.Users
            .Include(u => u.ClientProfile)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            throw new NotFoundException("الموكل غير موجود");

        if (!await _userManager.IsInRoleAsync(user, "Client"))
            throw new NotFoundException("المستخدم ليس موكلاً");

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

        if (user.ClientProfile == null)
        {
            user.ClientProfile = new ClientProfile { UserId = user.Id };
        }

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
            throw new NotFoundException("الموكل غير موجود");

        if (!await _userManager.IsInRoleAsync(user, "Client"))
            throw new NotFoundException("المستخدم ليس موكلاً");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new BusinessException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
