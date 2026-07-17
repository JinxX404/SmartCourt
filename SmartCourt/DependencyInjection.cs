using SmartCourt.Providers.Jwt;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Options;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartCourt.Features.Auth.ConfirmEmail;
using SmartCourt.Features.Auth.ChangePassword;
using SmartCourt.Features.Auth.ForgotPassword;
using SmartCourt.Features.Auth.ResetPassword;
using SmartCourt.Features.Auth.ResendVerification;
using SmartCourt.Features.Auth.Login;
using SmartCourt.Features.Auth.RefreshToken;
using SmartCourt.Features.Auth.RegisterClient;
using SmartCourt.Features.Auth.RegisterLawyer;
using SmartCourt.Features.Auth.RevokeRefreshToken;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using SmartCourt.Providers.FileStorage;
using static SmartCourt.Interfaces.Providers.IFileStorageService;

namespace SmartCourt;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<SmartCourt.Features.Auth.Login.Validators.LoginRequestValidator>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter your token in the text input below.",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
        services.Configure<SmartCourt.Providers.Email.MailKitOptions>(configuration.GetSection("SmtpSettings"));
        services.AddScoped<SmartCourt.Providers.Email.ISmtpEmailSender, SmartCourt.Providers.Email.SmtpEmailSender>();
        services.AddScoped<SmartCourt.Interfaces.Providers.IEmailProvider, SmartCourt.Providers.Email.BackgroundEmailProvider>();

        services.Configure<SmartCourt.Providers.Sms.TwilioOptions>(configuration.GetSection("Twilio"));
        if (isDevelopment)
        {
            services.AddScoped<SmartCourt.Providers.Sms.ISmsSender, SmartCourt.Providers.Sms.MockSmsSender>();
        }
        else
        {
            services.AddScoped<SmartCourt.Providers.Sms.ISmsSender, SmartCourt.Providers.Sms.TwilioSmsSender>();
        }
        services.AddScoped<SmartCourt.Interfaces.Providers.ISmsProvider, SmartCourt.Providers.Sms.BackgroundSmsProvider>();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"), new Hangfire.SqlServer.SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,
                PrepareSchemaIfNecessary = true
            }));
        services.AddHangfireServer();
        services.AddScoped<SmartCourt.Interfaces.Providers.IBackgroundJobProvider, SmartCourt.Providers.Jobs.HangfireJobProvider>();

        services.Configure<SupabaseOptions>(configuration.GetSection("Supabase"));
        services.AddScoped<IFileStorageService, SupabaseFileStorageService>();

        services.AddSingleton<Supabase.Client>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SupabaseOptions>>().Value;

            var client = new Supabase.Client(
                options.Url,
                options.ApiKey);

            client.InitializeAsync().GetAwaiter().GetResult();

            return client;
        });

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var key = Encoding.UTF8.GetBytes(jwtOptions.Secret ?? string.Empty);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

        services.AddAuthorization();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IAuthHelperService, AuthHelperService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IConfirmEmailService, ConfirmEmailService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IRegisterClientService, RegisterClientService>();
        services.AddScoped<IRegisterLawyerService, RegisterLawyerService>();
        services.AddScoped<IRevokeRefreshTokenService, RevokeRefreshTokenService>();
        services.AddScoped<IChangePasswordService, ChangePasswordService>();
        services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
        services.AddScoped<IResetPasswordService, ResetPasswordService>();
        services.AddScoped<IResendVerificationService, ResendVerificationService>();
        services.AddScoped<SmartCourt.Features.Users.Lawyers.ILawyerService, SmartCourt.Features.Users.Lawyers.LawyerService>();
        services.AddScoped<SmartCourt.Features.Users.Clients.IClientService, SmartCourt.Features.Users.Clients.ClientService>();

        return services;
    }
}
