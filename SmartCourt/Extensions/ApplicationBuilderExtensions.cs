using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCourt.Persistence;

namespace SmartCourt.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAutoMigration(this IApplicationBuilder app)
    {
        try 
        {
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (db.Database.IsSqlServer())
            {
                db.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            AppDomain.CurrentDomain.SetData("MigrationError", ex.ToString());
            throw;
        }

        return app;
    }
}
