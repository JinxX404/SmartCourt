using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartCourt.Persistence;
using Hangfire;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
        // Email Infrastructure Providers
        services.Configure<SmartCourt.Providers.Email.MailKitOptions>(configuration.GetSection("SmtpSettings"));
        if (isDevelopment)
        {
            services.AddScoped<SmartCourt.Providers.Email.ISmtpEmailSender, SmartCourt.Providers.Email.MockSmtpEmailSender>();
        }
        else
        {
            services.AddScoped<SmartCourt.Providers.Email.ISmtpEmailSender, SmartCourt.Providers.Email.SmtpEmailSender>();
        }
        services.AddScoped<SmartCourt.Interfaces.Providers.IEmailProvider, SmartCourt.Providers.Email.BackgroundEmailProvider>();

        // SMS Infrastructure Providers
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

        // Background Jobs (Hangfire)
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
            
        // In the future, we will register Repositories, Identity, and Email services here
            
        return services;
    }
}
