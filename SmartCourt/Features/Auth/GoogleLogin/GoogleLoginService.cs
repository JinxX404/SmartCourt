using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.GoogleLogin.DTOs;
using SmartCourt.Features.Auth.Login.DTOs;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Interfaces.Providers;
using Microsoft.Extensions.Configuration;
using SmartCourt.Persistence;
using System.Text.Json;
using System.Net.Http.Headers;

namespace SmartCourt.Features.Auth.GoogleLogin;

public interface IGoogleLoginService
{
    Task<LoginResponse> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default);
}

public class GoogleLoginService : IGoogleLoginService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtProvider _jwtProvider;
    private readonly IAuthHelperService _authHelper;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _dbContext;
    private readonly int _refreshTokenExpiryDays = 7;

    public GoogleLoginService(
        UserManager<ApplicationUser> userManager,
        IJwtProvider jwtProvider,
        IAuthHelperService authHelper,
        IConfiguration configuration,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _jwtProvider = jwtProvider;
        _authHelper = authHelper;
        _configuration = configuration;
        _dbContext = dbContext;
    }

    public async Task<LoginResponse> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default)
    {
        var clientId = _configuration["Google:ClientId"];
        if (string.IsNullOrEmpty(clientId))
        {
            throw new BusinessException("لم يتم إعداد خدمة حسابات جوجل على الخادم حتى الآن.");
        }

        string email;
        string name;

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.IdToken);
            
            var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new AuthenticationException("التوكن الخاص بجوجل غير صالح أو منتهي الصلاحية.");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var userInfo = JsonSerializer.Deserialize<JsonElement>(content);
            
            email = userInfo.GetProperty("email").GetString()!;
            name = userInfo.TryGetProperty("name", out var n) ? n.GetString() ?? "مستخدم جوجل" : "مستخدم جوجل";
        }
        catch (Exception ex) when (ex is not BusinessException && ex is not AuthenticationException)
        {
            throw new AuthenticationException("التوكن الخاص بجوجل غير صالح أو منتهي الصلاحية.");
        }

        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            if (string.IsNullOrEmpty(request.Role))
            {
                // This exception will be caught by the frontend and show a role selection dialog
                throw new BusinessException("ROLE_REQUIRED");
            }

            var roleName = request.Role.Equals("lawyer", StringComparison.OrdinalIgnoreCase) ? "Lawyer" : "Client";
            await _authHelper.EnsureRoleExistsAsync(roleName);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = name,
                    EmailConfirmed = true,
                    Status = UserStatus.Unverified
                };

                if (roleName == "Lawyer")
                {
                    user.LawyerProfile = new LawyerProfile { IsAvailable = true };
                }
                else
                {
                    user.ClientProfile = new ClientProfile();
                }

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    throw new BusinessException("فشل إنشاء الحساب بواسطة جوجل.");
                }

                await _userManager.AddToRoleAsync(user, roleName);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        else
        {
            if (user.Status == UserStatus.Suspended)
            {
                throw new ForbiddenAccessException("تم تعليق حسابك. تواصل مع الدعم");
            }

            if (user.Status == UserStatus.Deleted)
            {
                throw new AuthenticationException("البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            }
            
            // If the user registered manually and didn't confirm email, Google login confirms it automatically
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        var tokenResult = _jwtProvider.GenerateToken(user, roles);

        var refreshToken = _authHelper.GenerateRefreshToken();
        var hashedRefreshToken = _authHelper.HashRefreshToken(refreshToken);
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        user.RefreshTokens ??= new List<SmartCourt.Common.Entities.RefreshToken>();
        user.RefreshTokens.Add(new SmartCourt.Common.Entities.RefreshToken
        {
            HashedToken = hashedRefreshToken,
            ExpiresOn = refreshTokenExpiration
        });

        user.LastLoginAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        var userDto = new UserDto(
            user.Id.ToString(),
            user.Email ?? string.Empty,
            user.FullName,
            roles.FirstOrDefault() ?? "Client",
            user.Status.ToString()
        );

        return new LoginResponse(
            userDto,
            tokenResult.Token,
            tokenResult.ExpiresInSeconds,
            refreshToken,
            refreshTokenExpiration);
    }
}
