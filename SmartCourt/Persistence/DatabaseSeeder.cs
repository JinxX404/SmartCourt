using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SmartCourt.Features.Auth;
using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        /*
         * ALGORITHM:
         * 1. Create a service scope to resolve scoped services (RoleManager, UserManager).
         * 2. Ensure roles exist: "Client", "Lawyer", "Admin".
         * 3. Check if the "admin@smartcourt.com" user exists.
         * 4. If not, create a new ApplicationUser:
         *    - Email = "admin@smartcourt.com"
         *    - FullName = "System Administrator"
         *    - Status = UserStatus.Verified
         *    - EmailConfirmed = true
         * 5. Set password (e.g. "Admin@123").
         * 6. Add user to the "Admin" role.
         */
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var roles = new[] { "Client", "Lawyer", "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        var adminEmail = "admin@smartcourt.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                Status = UserStatus.Active,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
