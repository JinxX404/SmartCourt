using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.RegisterClient.DTOs;
using SmartCourt.Features.Auth.RegisterLawyer.DTOs;
using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Auth.RegisterLawyer;

public class RegisterLawyerService : IRegisterLawyerService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthHelperService _authHelper;
    private readonly ApplicationDbContext _dbContext;
    private readonly SmartCourt.Features.LawyerSubscription.ILawyerQuotaService _lawyerQuotaService;

    public RegisterLawyerService(
        UserManager<ApplicationUser> userManager,
        IAuthHelperService authHelper,
        ApplicationDbContext dbContext,
        SmartCourt.Features.LawyerSubscription.ILawyerQuotaService lawyerQuotaService)
    {
        _userManager = userManager;
        _authHelper = authHelper;
        _dbContext = dbContext;
        _lawyerQuotaService = lawyerQuotaService;
    }

    public async Task<RegisterResponse> RegisterLawyerAsync(RegisterLawyerRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new ValidationException("ConfirmPassword", "كلمة المرور وتأكيد كلمة المرور غير متطابقتين.");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ConflictException("البريد الإلكتروني مسجل بالفعل.");
        }

        await _authHelper.EnsureRoleExistsAsync("Lawyer");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                Status = UserStatus.Unverified,
                LawyerProfile = new LawyerProfile
                {
                    IsAvailable = true
                }
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                throw new ValidationException(result.Errors.Select(e => new KeyValuePair<string, string[]>(e.Code, new[] { e.Description })));
            }

            await _userManager.AddToRoleAsync(user, "Lawyer");
            
            // Auto-provision Free plan for Lawyer AI Tokens
            await _lawyerQuotaService.GetOrCreateSubscriptionAsync(user.Id, cancellationToken);
            
            await _authHelper.SendConfirmationEmailAsync(user, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return new RegisterResponse(user.Id.ToString(), user.Email!, user.FullName, "Lawyer");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
