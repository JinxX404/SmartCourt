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

        var user = await _userManager.Users
            .Include(u => u.LawyerProfile)
            .ThenInclude(lp => lp.Specializations)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new NotFoundException("المحامي غير موجود");

        var specializations = user.LawyerProfile?.Specializations
            .Select(s => new LawyerSpecializationDto
            {
                Specialization = s.Specialization,
                YearsOfExperience = s.YearsOfExperience,
                CasesHandled = s.CasesHandled
            }).ToList() ?? new List<LawyerSpecializationDto>();

        return new LawyerProfileResponse
        {
            Id = user.Id,
            Name = user.FullName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            NationalNumber = user.NationalNumber ?? string.Empty,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Address = user.Address,
            Governorate = user.Governorate,
            City = user.City,
            Status = user.Status.ToString(),
            Level = user.LawyerProfile != null ? user.LawyerProfile.Level : SmartCourt.Common.Enums.LawyerLevel.GeneralRegistration,
            Bio = user.LawyerProfile?.Bio,
            IsAvailable = user.LawyerProfile != null && user.LawyerProfile.IsAvailable,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Specializations = specializations
        };
    }

    public async Task<PublicLawyerProfileResponse> GetPublicProfileAsync(Guid lawyerId, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .WherePublicLawyer(lawyerId)
            .Include(u => u.LawyerProfile)
            .ThenInclude(lp => lp.Specializations)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            throw new NotFoundException("المحامي غير موجود");

        var specializations = user.LawyerProfile?.Specializations
            .Select(s => new LawyerSpecializationDto
            {
                Specialization = s.Specialization,
                YearsOfExperience = s.YearsOfExperience,
                CasesHandled = s.CasesHandled
            }).ToList() ?? new List<LawyerSpecializationDto>();

        return new PublicLawyerProfileResponse
        {
            Id = user.Id,
            Name = user.FullName ?? string.Empty,
            Gender = user.Gender,
            Level = user.LawyerProfile != null ? user.LawyerProfile.Level : SmartCourt.Common.Enums.LawyerLevel.GeneralRegistration,
            Bio = user.LawyerProfile?.Bio,
            Governorate = user.Governorate,
            City = user.City,
            IsAvailable = user.LawyerProfile != null && user.LawyerProfile.IsAvailable,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Specializations = specializations
        };
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
            user.Governorate = request.Governorate;
            user.City = request.City;
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

            if (request.Specializations != null && request.Specializations.Count > 0)
            {
                var existingSpecs = await _dbContext.LawyerSpecializations
                    .Where(ls => ls.LawyerProfileUserId == user.Id)
                    .ToListAsync(cancellationToken);

                if (existingSpecs.Count > 0)
                {
                    _dbContext.LawyerSpecializations.RemoveRange(existingSpecs);
                }

                foreach (var specDto in request.Specializations)
                {
                    _dbContext.LawyerSpecializations.Add(new LawyerSpecialization
                    {
                        Id = Guid.NewGuid(),
                        LawyerProfileUserId = user.Id,
                        Specialization = specDto.Specialization,
                        YearsOfExperience = specDto.YearsOfExperience,
                        CasesHandled = specDto.CasesHandled
                    });
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
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
            user.Governorate = request.Governorate;
            user.City = request.City;

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

            if (request.Specializations != null)
            {
                var existingSpecs = await _dbContext.LawyerSpecializations
                    .Where(ls => ls.LawyerProfileUserId == user.Id)
                    .ToListAsync(cancellationToken);

                if (existingSpecs.Count > 0)
                {
                    _dbContext.LawyerSpecializations.RemoveRange(existingSpecs);
                }

                foreach (var specDto in request.Specializations)
                {
                    _dbContext.LawyerSpecializations.Add(new LawyerSpecialization
                    {
                        Id = Guid.NewGuid(),
                        LawyerProfileUserId = user.Id,
                        Specialization = specDto.Specialization,
                        YearsOfExperience = specDto.YearsOfExperience,
                        CasesHandled = specDto.CasesHandled
                    });
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
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
