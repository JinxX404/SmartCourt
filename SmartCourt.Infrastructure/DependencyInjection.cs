using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartCourt.Infrastructure.Persistence;
using Hangfire;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
        // Email Infrastructure Providers
        services.Configure<SmartCourt.Infrastructure.Providers.Email.MailKitOptions>(configuration.GetSection("SmtpSettings"));
        if (isDevelopment)
        {
            services.AddScoped<SmartCourt.Infrastructure.Providers.Email.ISmtpEmailSender, SmartCourt.Infrastructure.Providers.Email.MockSmtpEmailSender>();
        }
        else
        {
            services.AddScoped<SmartCourt.Infrastructure.Providers.Email.ISmtpEmailSender, SmartCourt.Infrastructure.Providers.Email.SmtpEmailSender>();
        }
        services.AddScoped<SmartCourt.Core.Interfaces.Providers.IEmailProvider, SmartCourt.Infrastructure.Providers.Email.BackgroundEmailProvider>();

        // SMS Infrastructure Providers
        services.Configure<SmartCourt.Infrastructure.Providers.Sms.TwilioOptions>(configuration.GetSection("Twilio"));
        if (isDevelopment)
        {
            services.AddScoped<SmartCourt.Infrastructure.Providers.Sms.ISmsSender, SmartCourt.Infrastructure.Providers.Sms.MockSmsSender>();
        }
        else
        {
            services.AddScoped<SmartCourt.Infrastructure.Providers.Sms.ISmsSender, SmartCourt.Infrastructure.Providers.Sms.TwilioSmsSender>();
        }
        services.AddScoped<SmartCourt.Core.Interfaces.Providers.ISmsProvider, SmartCourt.Infrastructure.Providers.Sms.BackgroundSmsProvider>();

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
        services.AddScoped<SmartCourt.Core.Interfaces.Providers.IBackgroundJobProvider, SmartCourt.Infrastructure.Providers.Jobs.HangfireJobProvider>();
            
        // In the future, we will register Repositories, Identity, and Email services here
            
        return services;
    }
}
