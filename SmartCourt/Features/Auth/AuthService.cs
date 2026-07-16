using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SmartCourt.Common;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.Login;
using SmartCourt.Features.Auth.RegisterClient;
using SmartCourt.Features.Auth.RegisterLawyer;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using System.Text;

namespace SmartCourt.Features.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly JwtProvider _jwtProvider;
    private readonly IEmailProvider _emailProvider;
    private readonly string _appUrl;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        JwtProvider jwtProvider,
        IEmailProvider emailProvider,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtProvider = jwtProvider;
        _emailProvider = emailProvider;
        _appUrl = configuration["AppUrl"]?.TrimEnd('/') ?? "http://localhost:5000";
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new AuthenticationException("البريد الإلكتروني أو كلمة المرور غير صحيحة.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!signInResult.Succeeded)
        {
            throw new AuthenticationException("البريد الإلكتروني أو كلمة المرور غير صحيحة.");
        }

        if (!user.EmailConfirmed)
        {
            throw new ForbiddenAccessException("يرجى تأكيد البريد الإلكتروني أولاً");
        }

        if (user.Status == UserStatus.Suspended)
        {
            throw new ForbiddenAccessException("تم تعليق حسابك. تواصل مع الدعم");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var tokenResult = _jwtProvider.GenerateToken(user, roles);

        return new LoginResponse(
            user.Id.ToString(),
            user.Email ?? string.Empty,
            user.FullName,
            roles.FirstOrDefault() ?? "User",
            tokenResult.Token,
            tokenResult.ExpiresInSeconds);
    }

    public async Task<RegisterResponse> RegisterClientAsync(RegisterClientRequest request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new ValidationException("ConfirmPassword", "كلمة المرور وتأكيد كلمة المرور غير متطابقتين.");
        }

        await EnsureRoleExistsAsync("Client");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            NationalNumber = request.NationalNumber,
            Status = UserStatus.Unverified
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new KeyValuePair<string, string[]>(e.Code, new[] { e.Description })));
        }

        await _userManager.AddToRoleAsync(user, "Client");
        await SendConfirmationEmailAsync(user);

        return new RegisterResponse(user.Id.ToString(), user.Email!, user.FullName, "Client");
    }

    public async Task<RegisterResponse> RegisterLawyerAsync(RegisterLawyerRequest request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new ValidationException("ConfirmPassword", "كلمة المرور وتأكيد كلمة المرور غير متطابقتين.");
        }

        await EnsureRoleExistsAsync("Lawyer");

        var fullAddress = string.Join(", ", new[] { request.Address.Trim(), request.City.Trim(), request.Government.Trim() }.Where(x => !string.IsNullOrWhiteSpace(x)));

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.Phone,
            Address = fullAddress,
            Gender = request.Gender,
            NationalNumber = request.NationalNumber,
            Status = UserStatus.Unverified
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new KeyValuePair<string, string[]>(e.Code, new[] { e.Description })));
        }

        await _userManager.AddToRoleAsync(user, "Lawyer");
        await SendConfirmationEmailAsync(user);

        return new RegisterResponse(user.Id.ToString(), user.Email!, user.FullName, "Lawyer");
    }

    public async Task ConfirmEmailAsync(string userId, string token)
    {
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            throw new ValidationException("userId", "معرّف المستخدم غير صالح.");
        }

        var user = await _userManager.FindByIdAsync(parsedUserId.ToString());
        if (user is null)
        {
            throw new NotFoundException("المستخدم غير موجود.");
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            throw new BusinessException("رمز تأكيد البريد الإلكتروني غير صالح أو منتهي الصلاحية.");
        }
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmationUrl = $"{_appUrl}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        var subject = "تأكيد البريد الإلكتروني - المحكمة الذكية";
        var body = $@"
<!DOCTYPE html>
<html lang='ar' dir='rtl'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f7f6;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 40px auto;
            background: #ffffff;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.05);
            overflow: hidden;
            border: 1px solid #e1e8ed;
        }}
        .header {{
            background-color: #1a365d;
            color: #ffffff;
            padding: 30px 20px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
            font-weight: 600;
        }}
        .content {{
            padding: 40px 30px;
            color: #333333;
            line-height: 1.6;
            text-align: right;
        }}
        .content h2 {{
            color: #1a365d;
            font-size: 20px;
            margin-top: 0;
        }}
        .btn-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .btn {{
            display: inline-block;
            background-color: #d4af37;
            color: #ffffff;
            text-decoration: none;
            padding: 14px 32px;
            border-radius: 4px;
            font-weight: bold;
            font-size: 16px;
        }}
        .footer {{
            background-color: #f8fafc;
            padding: 20px;
            text-align: center;
            font-size: 13px;
            color: #64748b;
            border-top: 1px solid #e2e8f0;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>المحكمة الذكية</h1>
        </div>
        <div class='content'>
            <h2>مرحباً {user.FullName}،</h2>
            <p>شكراً لانضمامك إلى منصة المحكمة الذكية. يرجى تأكيد عنوان بريدك الإلكتروني لإكمال عملية التسجيل وتفعيل حسابك.</p>
            
            <div class='btn-container'>
                <a href='{confirmationUrl}' class='btn'>تأكيد البريد الإلكتروني</a>
            </div>
            
            <p>إذا لم تقم بإنشاء هذا الحساب، يرجى تجاهل هذه الرسالة.</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.UtcNow.Year} منصة المحكمة الذكية. جميع الحقوق محفوظة.</p>
        </div>
    </div>
</body>
</html>";

        await _emailProvider.SendEmailAsync(user.Email!, subject, body, true);
    }

    public Task ForgotPasswordAsync(string email)
    {
        /*
         * ALGORITHM:
         * 1. Find user by email (_userManager.FindByEmailAsync).
         * 2. If user doesn't exist, return immediately (don't throw error to prevent enumeration).
         * 3. Check if email is confirmed. If not, optionally return or send verification link instead.
         * 4. Generate reset token: _userManager.GeneratePasswordResetTokenAsync(user).
         * 5. URL encode token: WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token)).
         * 6. Construct reset URL: $"{_appUrl}/api/auth/reset-password?email={email}&token={encodedToken}".
         *    (Note: This will point to the frontend when a domain is available).
         * 7. Construct HTML email body and send via _emailProvider.SendEmailAsync.
         */
        throw new NotImplementedException();
    }

    public Task ResetPasswordAsync(string email, string token, string newPassword)
    {
        /*
         * ALGORITHM:
         * 1. Find user by email. If not found, throw BusinessException("بيانات غير صالحة").
         * 2. Decode the token: Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)).
         * 3. Call _userManager.ResetPasswordAsync(user, decodedToken, newPassword).
         * 4. If result is not Succeeded, throw ValidationException with errors.
         */
        throw new NotImplementedException();
    }

    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new AuthenticationException("المستخدم غير معروف");
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            throw new ValidationException("Password", string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }

    public Task ResendVerificationEmailAsync(string email)
    {
        /*
         * ALGORITHM:
         * 1. Find user by email. If not found, return immediately (don't throw error).
         * 2. If user.EmailConfirmed == true, return immediately.
         * 3. Check rate limiting (e.g., max 3 per hour) if implemented in DB or Cache.
         * 4. Call SendConfirmationEmailAsync(user) to resend the email.
         */
        throw new NotImplementedException();
    }
}
