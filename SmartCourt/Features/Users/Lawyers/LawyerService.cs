using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Users.Lawyers.DTOs;
using SmartCourt.Persistence;
using SmartCourt.Interfaces;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Users.Shared.DTOs;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Features.Users.Lawyers;

public class LawyerService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IAuthHelperService authHelperService,
    IFileStorageService fileStorageService) : ILawyerService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IAuthHelperService _authHelperService = authHelperService;
    private readonly IFileStorageService _fileStorageService = fileStorageService;

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
            RejectionReason = user.RejectionReason,
            AverageRating = user.LawyerProfile?.AverageRating ?? 0m,
            RatingCount = user.LawyerProfile?.TotalRatingCount ?? 0,
            Specializations = specializations,
            YearsOfExperience = specializations.FirstOrDefault()?.YearsOfExperience ?? 0,
            SpecializationName = specializations.FirstOrDefault()?.Specialization.ToString()
        };
    }

    public async Task<PublicLawyerProfileResponse> GetPublicProfileAsync(Guid lawyerId, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .WherePublicLawyer(lawyerId)
            .Include(u => u.LawyerProfile)
            .ThenInclude(lp => lp!.Specializations)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            throw new NotFoundException("المحامي غير موجود");

        return MapToPublicDto(user);
    }

    public async Task<List<PublicLawyerProfileResponse>> GetTopLawyersAsync(CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(u =>
                u.LawyerProfile != null &&
                u.EmailConfirmed &&
                u.Status == UserStatus.Active)
            .OrderByDescending(u => u.LawyerProfile!.TotalRatingCount)
            .ThenByDescending(u => (double)u.LawyerProfile!.AverageRating)
            .Include(u => u.LawyerProfile)
                .ThenInclude(lp => lp!.Specializations)
            .Take(3)
            .ToListAsync(cancellationToken);

        return users.Select(MapToPublicDto).ToList();
    }

    public async Task<PagedResponse<List<PublicLawyerProfileResponse>>> SearchLawyersAsync(
        SearchLawyersRequest request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .Where(u =>
                u.LawyerProfile != null &&
                u.EmailConfirmed &&
                u.Status == UserStatus.Active);

        // Apply optional filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(term) ||
                (u.LawyerProfile!.Bio != null && u.LawyerProfile.Bio.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(request.Governorate))
            query = query.Where(u => u.Governorate == request.Governorate);

        if (request.Level.HasValue)
            query = query.Where(u => u.LawyerProfile!.Level == request.Level.Value);

        if (request.Specialization.HasValue)
            query = query.Where(u =>
                u.LawyerProfile!.Specializations.Any(s => s.Specialization == request.Specialization.Value));

        if (request.MinRating.HasValue)
            query = query.Where(u => u.LawyerProfile!.AverageRating >= request.MinRating.Value);

        if (request.IsAvailable.HasValue)
            query = query.Where(u => u.LawyerProfile!.IsAvailable == request.IsAvailable.Value);

        // Apply sorting
        query = (request.SortBy, request.SortDirection) switch
        {
            (LawyerSortBy.Rating, SortDirection.Descending)    => query.OrderByDescending(u => u.LawyerProfile!.AverageRating),
            (LawyerSortBy.Rating, SortDirection.Ascending)     => query.OrderBy(u => u.LawyerProfile!.AverageRating),
            (LawyerSortBy.ResponseTime, SortDirection.Ascending)  => query.OrderBy(u => u.LawyerProfile!.AverageResponseTimeHours),
            (LawyerSortBy.ResponseTime, SortDirection.Descending) => query.OrderByDescending(u => u.LawyerProfile!.AverageResponseTimeHours),
            (LawyerSortBy.ExperienceLevel, SortDirection.Descending) => query.OrderByDescending(u => u.LawyerProfile!.Level),
            (LawyerSortBy.ExperienceLevel, SortDirection.Ascending)  => query.OrderBy(u => u.LawyerProfile!.Level),
            _ => query.OrderByDescending(u => u.LawyerProfile!.AverageRating)
        };

        var totalRecords = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        var users = await query
            .Include(u => u.LawyerProfile)
                .ThenInclude(lp => lp!.Specializations)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = users.Select(MapToPublicDto).ToList();

        return PagedResponse<List<PublicLawyerProfileResponse>>.OkPaged(
            items, request.PageNumber, request.PageSize, totalRecords, totalPages);
    }

    private static PublicLawyerProfileResponse MapToPublicDto(ApplicationUser user)
    {
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
            AverageRating = user.LawyerProfile?.AverageRating ?? 0m,
            RatingCount = user.LawyerProfile?.TotalRatingCount ?? 0,
            Specializations = specializations,
            YearsOfExperience = specializations.FirstOrDefault()?.YearsOfExperience ?? 0,
            SpecializationName = specializations.FirstOrDefault()?.Specialization.ToString()
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

        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        if (request.DateOfBirth > today.AddYears(-21))
        {
            throw new BusinessException("يجب أن يكون عمر المستخدم 21 عاماً أو أكثر.");
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
                .ThenInclude(lp => lp.Specializations)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new NotFoundException("المحامي غير موجود");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var modifiedFields = new List<string>();

            if (user.Address != request.Address) modifiedFields.Add("Address");
            if (user.DateOfBirth != request.DateOfBirth) modifiedFields.Add("DateOfBirth");
            if (request.Gender.HasValue && user.Gender != request.Gender.Value) modifiedFields.Add("Gender");
            if (user.NationalNumber != request.NationalNumber) modifiedFields.Add("NationalNumber");
            if (user.Governorate != request.Governorate) modifiedFields.Add("Governorate");
            if (user.City != request.City) modifiedFields.Add("City");
            if (user.LawyerProfile?.Level != request.Level) modifiedFields.Add("Level");
            if (user.LawyerProfile?.Bio != request.Bio) modifiedFields.Add("Bio");

            if (request.Specializations != null && request.Specializations.Any())
                modifiedFields.Add("Specializations"); // We assume they changed if provided in the request

            if (modifiedFields.Any())
            {
                var currentModified = string.IsNullOrEmpty(user.ModifiedFieldsJson) 
                    ? new List<string>() 
                    : System.Text.Json.JsonSerializer.Deserialize<List<string>>(user.ModifiedFieldsJson) ?? new List<string>();
                
                currentModified.AddRange(modifiedFields);
                user.ModifiedFieldsJson = System.Text.Json.JsonSerializer.Serialize(currentModified.Distinct().ToList());
            }

            user.Address = request.Address;
            user.DateOfBirth = request.DateOfBirth;
            if (request.Gender.HasValue) user.Gender = request.Gender.Value;
            user.NationalNumber = request.NationalNumber;
            user.Governorate = request.Governorate;
            user.City = request.City;

            if (user.LawyerProfile == null)
            {
                user.LawyerProfile = new LawyerProfile { UserId = user.Id, IsAvailable = true };
            }

            user.LawyerProfile.Level = request.Level;
            user.LawyerProfile.Bio = request.Bio;



            if (user.Status == UserStatus.Active || user.Status == UserStatus.Rejected)
            {
                user.Status = UserStatus.PendingReview;
            }

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
            else
            {
                // Ensure tracked changes to related entities (like LawyerProfile, Specializations) are saved
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

        if (request.DateOfBirth.HasValue && request.DateOfBirth.Value > DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date).AddYears(-21))
        {
            throw new BusinessException("يجب أن يكون عمر المستخدم 21 عاماً أو أكثر.");
        }
    }

    public async Task<LawyerAvailabilityResponse> SwitchAvailabilityAsync(
        UpdateLawyerAvailabilityRequest? request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var user = await _userManager.Users
            .Include(u => u.LawyerProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || user.Status == UserStatus.Deleted)
            throw new NotFoundException("المحامي غير موجود");

        var currentAvailability = user.LawyerProfile?.IsAvailable ?? false;
        var targetAvailability = request?.IsAvailable ?? !currentAvailability;

        if (user.LawyerProfile == null)
        {
            user.LawyerProfile = new LawyerProfile
            {
                UserId = user.Id,
                IsAvailable = targetAvailability
            };
            _dbContext.LawyerProfiles.Add(user.LawyerProfile);
        }
        else
        {
            user.LawyerProfile.IsAvailable = targetAvailability;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LawyerAvailabilityResponse
        {
            LawyerId = user.Id,
            IsAvailable = user.LawyerProfile.IsAvailable
        };
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

        // Check for associated cases/contracts
        var hasContracts = await _dbContext.Contracts.AnyAsync(c => c.LawyerUserId == userId, cancellationToken);
        if (hasContracts)
        {
            throw new BusinessException("لا يمكن حذف الحساب لوجود قضايا وعقود مرتبطة به.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Fetch and delete verification documents from storage
            var verificationDocs = await _dbContext.UserVerificationDocuments
                .Include(d => d.StoredFile)
                .Where(d => d.UserId == userId)
                .ToListAsync(cancellationToken);

            foreach (var doc in verificationDocs)
            {
                if (doc.StoredFile != null && !string.IsNullOrEmpty(doc.StoredFile.FileUrl))
                {
                    try { await _fileStorageService.DeleteAsync(doc.StoredFile.FileUrl, cancellationToken); } catch { /* Ignore if already deleted */ }
                }
            }

            // Delete profile picture if any
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && user.ProfilePictureUrl.Contains("supabase"))
            {
                // We assume it's just the filename/path. If it's a full URL, IFileStorageService might need the relative path. 
                // For safety, assuming it works or we catch exception
                try { await _fileStorageService.DeleteAsync(user.ProfilePictureUrl, cancellationToken); } catch { }
            }

            // Remove documents from DB
            _dbContext.UserVerificationDocuments.RemoveRange(verificationDocs);

            // Remove specializations
            var specializations = await _dbContext.LawyerSpecializations
                .Where(s => s.LawyerProfileUserId == userId)
                .ToListAsync(cancellationToken);
            _dbContext.LawyerSpecializations.RemoveRange(specializations);

            // Remove Profile
            if (user.LawyerProfile != null)
            {
                _dbContext.LawyerProfiles.Remove(user.LawyerProfile);
            }

            _authHelperService.RevokeAllActiveRefreshTokens(user);

            // Delete ApplicationUser (Cascade deletes should handle RefreshTokens)
            var deleteResult = await _userManager.DeleteAsync(user);
            EnsureSucceeded(deleteResult);

            await _dbContext.SaveChangesAsync(cancellationToken);
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
