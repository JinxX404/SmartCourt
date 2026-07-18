using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartCourt.Extensions;
using SmartCourt.Middleware;

namespace SmartCourt
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Add API Services
            builder.Services.AddApiServices();

            // 2. Add Infrastructure Services (Database, Identity, Email, etc.)
            builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment.IsDevelopment());
            

            var app = builder.Build();

            // 3. Configure HTTP Request Pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseHangfireDashboard();

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // 4. Auto-Migrate Database on Startup
            app.UseAutoMigration();

            // 5. Seed Database
            using (var scope = app.Services.CreateScope())
            {
                SmartCourt.Persistence.DatabaseSeeder.SeedAsync(scope.ServiceProvider).GetAwaiter().GetResult();
            }

            app.Run();
        }
    }
}
