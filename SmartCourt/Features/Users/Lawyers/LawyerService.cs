using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Users.Lawyers.DTOs;
using SmartCourt.Persistence;
using SmartCourt.Interfaces;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Features.Users.Lawyers;

public class LawyerService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IAuthHelperService authHelperService) : ILawyerService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IAuthHelperService _authHelperService = authHelperService;

    public async Task<LawyerProfileResponse> GetProfileAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var response = await _userManager.Users
            .Where(u => u.Id == userId)
            .Select(u => new LawyerProfileResponse
            {
                Id = u.Id,
                Name = u.FullName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                NationalNumber = u.NationalNumber ?? string.Empty,
                Gender = u.Gender ?? string.Empty,
                DateOfBirth = u.DateOfBirth,
                Address = u.Address,
                Status = u.Status.ToString(),
                SpecializationId = u.LawyerProfile != null ? u.LawyerProfile.SpecializationId : null,
                SpecializationName = u.LawyerProfile != null && u.LawyerProfile.Specialization != null ? u.LawyerProfile.Specialization.Name : string.Empty,
                CategoryName = u.LawyerProfile != null && u.LawyerProfile.Specialization != null && u.LawyerProfile.Specialization.Category != null ? u.LawyerProfile.Specialization.Category.Name : string.Empty,
                YearsOfExperience = u.LawyerProfile != null ? u.LawyerProfile.YearsOfExperience : 0,
                Bio = u.LawyerProfile != null ? u.LawyerProfile.Bio : null,
                IsAvailable = u.LawyerProfile != null && u.LawyerProfile.IsAvailable,
                ProfilePictureUrl = u.ProfilePictureUrl
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (response == null)
            throw new NotFoundException("المحامي غير موجود");

        return response;
    }

    public async Task<PublicLawyerProfileResponse> GetPublicProfileAsync(Guid lawyerId, CancellationToken cancellationToken)
    {
        var response = await _userManager.Users
            .Where(u => u.Id == lawyerId)
            .Select(u => new PublicLawyerProfileResponse
            {
                Id = u.Id,
                Name = u.FullName ?? string.Empty,
                Gender = u.Gender ?? string.Empty,
                Status = u.Status.ToString(),
                SpecializationId = u.LawyerProfile != null ? u.LawyerProfile.SpecializationId : null,
                SpecializationName = u.LawyerProfile != null && u.LawyerProfile.Specialization != null ? u.LawyerProfile.Specialization.Name : string.Empty,
                CategoryName = u.LawyerProfile != null && u.LawyerProfile.Specialization != null && u.LawyerProfile.Specialization.Category != null ? u.LawyerProfile.Specialization.Category.Name : string.Empty,
                YearsOfExperience = u.LawyerProfile != null ? u.LawyerProfile.YearsOfExperience : 0,
                Bio = u.LawyerProfile != null ? u.LawyerProfile.Bio : null,
                IsAvailable = u.LawyerProfile != null && u.LawyerProfile.IsAvailable,
                ProfilePictureUrl = u.ProfilePictureUrl
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (response == null)
            throw new NotFoundException("المحامي غير موجود");

        return response;
    }

    public async Task UpdateProfileAsync(UpdateLawyerProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var user = await _userManager.Users
            .Include(u => u.LawyerProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new NotFoundException("المحامي غير موجود");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            if (user.Email != request.Email)
            {
                await _authHelperService.SendChangeEmailConfirmationAsync(user, request.Email, cancellationToken);
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
                user.LawyerProfile = new LawyerProfile { UserId = user.Id, IsAvailable = true };
            }

            user.LawyerProfile.SpecializationId = request.SpecializationId;
            user.LawyerProfile.YearsOfExperience = request.YearsOfExperience;
            user.LawyerProfile.Bio = request.Bio;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new BusinessException(string.Join(" ", updateResult.Errors.Select(e => e.Description)));
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteProfileAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new NotFoundException("المحامي غير موجود");

        user.Status = UserStatus.Deleted;
        _authHelperService.RevokeAllActiveRefreshTokens(user);

        var updateResult = await _userManager.UpdateAsync(user);
        
        if (!updateResult.Succeeded)
        {
            throw new BusinessException(string.Join(" ", updateResult.Errors.Select(e => e.Description)));
        }
    }
}
