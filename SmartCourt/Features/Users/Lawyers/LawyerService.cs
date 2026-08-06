using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Users.Lawyers.DTOs;
using SmartCourt.Persistence;
using SmartCourt.Interfaces;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Users.Shared.DTOs;

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
                Gender = u.Gender,
                DateOfBirth = u.DateOfBirth,
                Address = u.Address,
                Status = u.Status.ToString(),
                Level = u.LawyerProfile != null ? u.LawyerProfile.Level : SmartCourt.Common.Enums.LawyerLevel.GeneralRegistration,
                YearsOfExperience = u.LawyerProfile != null && u.LawyerProfile.Specializations.Any() ? u.LawyerProfile.Specializations.First().YearsOfExperience : 1,
                SpecializationName = u.LawyerProfile != null && u.LawyerProfile.Specializations.Any() ? u.LawyerProfile.Specializations.First().Specialization.ToString() : "محاماة عامة",
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
            .WherePublicLawyer(lawyerId)
            .Select(u => new PublicLawyerProfileResponse
            {
                Id = u.Id,
                Name = u.FullName ?? string.Empty,
                Gender = u.Gender,
                Level = u.LawyerProfile != null ? u.LawyerProfile.Level : SmartCourt.Common.Enums.LawyerLevel.GeneralRegistration,
                Bio = u.LawyerProfile != null ? u.LawyerProfile.Bio : null,
                IsAvailable = u.LawyerProfile != null && u.LawyerProfile.IsAvailable,
                ProfilePictureUrl = u.ProfilePictureUrl
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (response == null)
            throw new NotFoundException("المحامي غير موجود");

        return response;
    }

    public async Task CompleteProfileAsync(CompleteLawyerProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var user = await _userManager.Users
            .Include(u => u.LawyerProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new NotFoundException("المحامي غير موجود");

        if (user.Status == UserStatus.Active)
        {
            throw new BusinessException("تم استكمال الملف الشخصي مسبقاً.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            if (user.PhoneNumber != request.PhoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, request.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                    throw new BusinessException(string.Join(" ", setPhoneResult.Errors.Select(e => e.Description)));
            }

            user.NationalNumber = request.NationalNumber;
            user.Gender = request.Gender;
            user.DateOfBirth = request.DateOfBirth;
            user.Address = request.Address;
            user.Status = UserStatus.PendingReview;

            if (user.LawyerProfile == null)
            {
                user.LawyerProfile = new LawyerProfile { UserId = user.Id, IsAvailable = true };
            }

            user.LawyerProfile.Level = request.Level;
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

    public async Task UpdateProfileAsync(UpdateLawyerProfileRequest request, CancellationToken cancellationToken)
    {
        ValidateProfileRequest(request);

        var userId = _currentUserService.UserId;
        var user = await _userManager.Users
            .Include(u => u.LawyerProfile)
                .ThenInclude(lp => lp.Specializations)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new NotFoundException("المحامي غير موجود");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            if (user.PhoneNumber != request.PhoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, request.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                    throw new BusinessException(string.Join(" ", setPhoneResult.Errors.Select(e => e.Description)));
            }

            user.Address = request.Address;
            user.DateOfBirth = request.DateOfBirth;

            if (user.LawyerProfile == null)
            {
                user.LawyerProfile = new LawyerProfile { UserId = user.Id, IsAvailable = true };
            }

            user.LawyerProfile.Level = request.Level;
            user.LawyerProfile.Bio = request.Bio;

            if (request.YearsOfExperience > 0)
            {
                var spec = user.LawyerProfile.Specializations.FirstOrDefault();
                if (spec == null)
                {
                    spec = new LawyerSpecialization
                    {
                        LawyerProfileUserId = user.Id,
                        Specialization = SmartCourt.Common.Enums.Specialization.CivilLaw,
                        YearsOfExperience = request.YearsOfExperience
                    };
                    user.LawyerProfile.Specializations.Add(spec);
                }
                else
                {
                    spec.YearsOfExperience = request.YearsOfExperience;
                }
            }

            if (user.Status == UserStatus.Active)
            {
                user.Status = UserStatus.PendingReview;
            }

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

    private static void ValidateProfileRequest(UpdateLawyerProfileRequest request)
    {
        if (!Enum.IsDefined(typeof(LawyerLevel), request.Level))
        {
            throw new ValidationException(nameof(request.Level), "مستوى المحامي غير صالح.");
        }

    }

    public async Task DeleteProfileAsync(
        DeleteAccountRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.LawyerProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || user.Status == UserStatus.Deleted)
            return;

        if (!await _userManager.CheckPasswordAsync(user, request.CurrentPassword))
        {
            throw new BusinessException("كلمة المرور الحالية غير صحيحة.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            user.Status = UserStatus.Deleted;
            if (user.LawyerProfile is not null)
            {
                user.LawyerProfile.IsAvailable = false;
            }

            _authHelperService.RevokeAllActiveRefreshTokens(user);

            var securityStampResult = await _userManager.UpdateSecurityStampAsync(user);
            EnsureSucceeded(securityStampResult);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new BusinessException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }
    }
}
