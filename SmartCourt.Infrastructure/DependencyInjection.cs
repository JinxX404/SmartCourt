using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartCourt.Infrastructure.Persistence;
using Hangfire;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
        // Infrastructure Providers
        services.Configure<SmartCourt.Infrastructure.Providers.Email.MailKitOptions>(configuration.GetSection("SmtpSettings"));
        services.AddScoped<SmartCourt.Infrastructure.Providers.Email.ISmtpEmailSender, SmartCourt.Infrastructure.Providers.Email.SmtpEmailSender>();
        services.AddScoped<SmartCourt.Core.Interfaces.Providers.IEmailProvider, SmartCourt.Infrastructure.Providers.Email.BackgroundEmailProvider>();

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
