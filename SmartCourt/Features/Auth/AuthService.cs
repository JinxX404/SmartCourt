using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SmartCourt.Common;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using System.Text;

namespace SmartCourt.Features.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailProvider _emailProvider;
    private readonly string _appUrl;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IEmailProvider emailProvider,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _emailProvider = emailProvider;
        _appUrl = configuration["AppUrl"]?.TrimEnd('/') ?? "http://localhost:5000";
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return;
        }

        if (!user.EmailConfirmed)
        {
            throw new BusinessException("البريد الإلكتروني غير مؤكد");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetUrl = $"{_appUrl}/api/auth/reset-password?email={email}&token={encodedToken}";

        var subject = "إعادة تعيين كلمة المرور - المحكمة الذكية";
        var body = $@"""
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
            <p>لقد تلقينا طلباً لإعادة تعيين كلمة المرور لحسابك. يرجى النقر على الرابط أدناه لتأكيد هويتك وتعيين كلمة مرور جديدة.</p>
            
            <div class='btn-container'>
                <a href='{resetUrl}' class='btn'>إعادة تعيين كلمة المرور</a>
            </div>
            
            <p>إذا لم تقم بطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذه الرسالة. سيظل حسابك آمناً.</p>
            <p>إذا استمرت المشكلة، يرجى التواصل مع الدعم.</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.UtcNow.Year} منصة المحكمة الذكية. جميع الحقوق محفوظة.</p>
        </div>
    </div>
</body>
</html>
        """;

        await _emailProvider.SendEmailAsync(user.Email!, subject, body, true);
    }

    public async Task ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new BusinessException("بيانات غير صالحة");
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
        if (!result.Succeeded)
        {
            throw new ValidationException("Password", string.Join(" ", result.Errors.Select(e => e.Description)));
        }
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

    public async Task ResendVerificationEmailAsync(string email)
    {

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return;
        }

        if (user.EmailConfirmed)
        {
            return;
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmationUrl = $"{_appUrl}/api/auth/confirm-email?email={email}&token={encodedToken}";

        var subject = "تأكيد البريد الإلكتروني - المحكمة الذكية";
        var body = $@"""
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
</html>
        """;

        await _emailProvider.SendEmailAsync(user.Email!, subject, body, true);
    }
}
