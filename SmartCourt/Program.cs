using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartCourt.Extensions;
using SmartCourt.Features.Chat.Hubs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Middleware;

namespace SmartCourt
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // 1. Add API Services
                builder.Services.AddApiServices();

                // 2. Add Infrastructure Services (Database, Identity, Email, etc.)
                builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment.IsDevelopment());
                

                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    scope.ServiceProvider
                        .GetRequiredService<
                            IPaymentProviderStartupValidator>()
                        .Validate();
                }

                // 3. Configure HTTP Request Pipeline
                app.UseMiddleware<ExceptionHandlingMiddleware>();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                    app.UseHangfireDashboard();
                }

                app.UseAuthentication();
                //app.UseRateLimiter();
                app.UseAuthorization();
                app.MapControllers();
                app.MapHealthChecks("/health");
                app.MapHub<ChatHub>("/hubs/chat").RequireAuthorization();

                // 4. Auto-Migrate Database on Startup
                app.UseAutoMigration();

                // 5. Seed Database
                using (var scope = app.Services.CreateScope())
                {
                    await SmartCourt.Persistence.DatabaseSeeder.SeedAsync(
                        scope.ServiceProvider);
                    await scope.ServiceProvider
                        .GetRequiredService<
                            SmartCourt.Infrastructure.Providers.Jobs
                                .IContractRecurringJobRegistrar>()
                        .RegisterAsync(app.Lifetime.ApplicationStopping);
                }

                app.Run();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("startup-error.txt", ex.ToString());
                throw;
            }
        }
    }
}
