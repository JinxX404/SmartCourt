using System.Text;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartCourt.Common;
using SmartCourt.Features.Auth;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using SmartCourt.Providers.FileStorage;
using static SmartCourt.Interfaces.Providers.IFileStorageService;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
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
        services.AddScoped<JwtProvider>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
