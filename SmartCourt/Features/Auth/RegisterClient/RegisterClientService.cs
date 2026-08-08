using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.RegisterClient.DTOs;
using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Auth.RegisterClient;

public class RegisterClientService : IRegisterClientService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthHelperService _authHelper;
    private readonly ApplicationDbContext _dbContext;

    public RegisterClientService(
        UserManager<ApplicationUser> userManager,
        IAuthHelperService authHelper,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _authHelper = authHelper;
        _dbContext = dbContext;
    }

    public async Task<RegisterResponse> RegisterClientAsync(RegisterClientRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new ValidationException("ConfirmPassword", "كلمة المرور وتأكيد كلمة المرور غير متطابقتين.");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            if (!existingUser.EmailConfirmed)
            {
                throw new ConflictException("البريد الإلكتروني مسجل بالفعل ولكنه غير مفعل. يرجى مراجعة بريدك الإلكتروني لتفعيل الحساب أو طلب رابط تفعيل جديد.");
            }
            throw new ConflictException("البريد الإلكتروني مسجل بالفعل.");
        }

        await _authHelper.EnsureRoleExistsAsync("Client");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                Status = UserStatus.Unverified,
                ClientProfile = new ClientProfile()
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                throw new ValidationException(result.Errors.Select(e => new KeyValuePair<string, string[]>(e.Code, new[] { e.Description })));
            }

            await _userManager.AddToRoleAsync(user, "Client");
            await _authHelper.SendConfirmationEmailAsync(user, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return new RegisterResponse(user.Id.ToString(), user.Email!, user.FullName, "Client");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
