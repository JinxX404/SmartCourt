using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartCourt.API.Middleware;
using SmartCourt.API.Extensions;
using Hangfire;

namespace SmartCourt.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Add API Services
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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
            app.UseAuthorization();
            app.MapControllers();

            // 4. Auto-Migrate Database on Startup
            app.UseAutoMigration();

            app.Run();
        }
    }
}
