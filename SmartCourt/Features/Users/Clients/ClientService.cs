using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Users.Clients.DTOs;
using SmartCourt.Persistence;
using SmartCourt.Interfaces;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Users.Shared.DTOs;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Features.Users.Clients;

public class ClientService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IAuthHelperService authHelperService,
    IFileStorageService fileStorageService) : IClientService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IAuthHelperService _authHelperService = authHelperService;
    private readonly IFileStorageService _fileStorageService = fileStorageService;

    public async Task<ClientProfileResponse> GetProfileAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var response = await _userManager.Users
            .Where(u => u.Id == userId)
            .Select(u => new ClientProfileResponse
            {
                Id = u.Id,
                Name = u.FullName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                NationalNumber = u.NationalNumber ?? string.Empty,
                Gender = u.Gender,
                DateOfBirth = u.DateOfBirth,
                Address = u.Address,
                Governorate = u.Governorate,
                City = u.City,
                Status = u.Status.ToString(),
                RejectionReason = u.RejectionReason
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (response == null)
            throw new NotFoundException("الموكل غير موجود");

        return response;
    }

    public async Task CompleteProfileAsync(CompleteClientProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var user = await _userManager.Users
            .Include(u => u.ClientProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new NotFoundException("الموكل غير موجود");

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

            user.DateOfBirth = request.DateOfBirth;
            user.Gender = request.Gender;
            user.Address = request.Address;
            user.NationalNumber = request.NationalNumber;
            user.Governorate = request.Governorate;
            user.City = request.City;
            user.Status = UserStatus.PendingReview;

            if (user.ClientProfile == null)
            {
                user.ClientProfile = new ClientProfile { UserId = user.Id };
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

    public async Task UpdateProfileAsync(UpdateClientProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var user = await _userManager.Users
            .Include(u => u.ClientProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new NotFoundException("الموكل غير موجود");

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

            if (user.ClientProfile == null)
            {
                user.ClientProfile = new ClientProfile { UserId = user.Id };
            }

            if (user.Status == UserStatus.Active || user.Status == UserStatus.Rejected)
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

    public async Task DeleteProfileAsync(
        DeleteAccountRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.ClientProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || user.Status == UserStatus.Deleted)
            return;

        if (!await _userManager.CheckPasswordAsync(user, request.CurrentPassword))
        {
            throw new BusinessException("كلمة المرور الحالية غير صحيحة.");
        }

        // Check for associated cases
        var hasCases = await _dbContext.LegalCases.AnyAsync(c => c.ClientUserId == userId, cancellationToken);
        if (hasCases)
        {
            throw new BusinessException("لا يمكن حذف الحساب لوجود قضايا مرتبطة به.");
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
                try { await _fileStorageService.DeleteAsync(user.ProfilePictureUrl, cancellationToken); } catch { }
            }

            // Remove Client Profile
            if (user.ClientProfile != null)
            {
                _dbContext.ClientProfile.Remove(user.ClientProfile);
            }

            _authHelperService.RevokeAllActiveRefreshTokens(user);

            // Hard delete user
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
