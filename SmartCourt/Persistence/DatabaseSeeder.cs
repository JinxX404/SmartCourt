using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
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
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await DataSeeders.LegalCategorySeeder.SeedAsync(context);

        var roles = new[] { "SuperAdministrator", "Client", "Lawyer", "Admin" };
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
                NationalNumber = "00000000000001",
                Status = UserStatus.Active,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        var superAdminEmail = "superadmin@smartcourt.com";
        var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

        if (superAdminUser == null)
        {
            superAdminUser = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                FullName = "Super Administrator",
                NationalNumber = "11111111111111",
                Status = UserStatus.Active,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(superAdminUser, "SuperAdmin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdminUser, "SuperAdministrator");
            }
        }

        var kokkerEmail = "kokker@gmail.com";
        var kokkerUser = await userManager.FindByEmailAsync(kokkerEmail);

        if (kokkerUser == null)
        {
            kokkerUser = new ApplicationUser
            {
                UserName = kokkerEmail,
                Email = kokkerEmail,
                FullName = "Ahmed Kokker",
                NationalNumber = "00000000000099",
                Status = UserStatus.Active,
                EmailConfirmed = true
            };

            var kokkerResult = await userManager.CreateAsync(kokkerUser, "Kokker@123");
            if (kokkerResult.Succeeded)
            {
                await userManager.AddToRoleAsync(kokkerUser, "Admin");
            }
        }

        var moatazEmail = "moatazmohammed2392003@gmail.com";
        var moatazUser = await userManager.FindByEmailAsync(moatazEmail);

        if (moatazUser == null)
        {
            moatazUser = new ApplicationUser
            {
                UserName = moatazEmail,
                Email = moatazEmail,
                FullName = "Moataz Mohammed",
                NationalNumber = "00000000000002",
                Status = UserStatus.Active,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(moatazUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(moatazUser, "Admin");
            }
        }

        var lawyerEmail = "lawyer@smartcourt.com";
        var lawyerUser = await userManager.FindByEmailAsync(lawyerEmail);

        if (lawyerUser == null)
        {
            lawyerUser = new ApplicationUser
            {
                UserName = lawyerEmail,
                Email = lawyerEmail,
                FullName = "Test Lawyer",
                PhoneNumber = "01000000000",
                NationalNumber = "00000000000003",
                Gender = Gender.Male,
                DateOfBirth = new DateOnly(1980, 1, 1),
                Address = "123 Legal St",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                LawyerProfile = new LawyerProfile
                {
                    Bio = "Experienced corporate lawyer.",
                }
            };

            var result = await userManager.CreateAsync(lawyerUser, "Lawyer@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(lawyerUser, "Lawyer");
            }
        }
        else
        {
            // Update existing record if it was already seeded from the previous run
            lawyerUser.PhoneNumber = "01000000000";
            lawyerUser.Gender = Gender.Male;
            lawyerUser.DateOfBirth = new DateOnly(1980, 1, 1);
            lawyerUser.Address = "123 Legal St";
            
            await userManager.UpdateAsync(lawyerUser);
        }

        var clientEmail = "client@smartcourt.com";
        var clientUser = await userManager.FindByEmailAsync(clientEmail);

        if (clientUser == null)
        {
            clientUser = new ApplicationUser
            {
                UserName = clientEmail,
                Email = clientEmail,
                FullName = "Test Client",
                PhoneNumber = "01100000000",
                NationalNumber = "00000000000004",
                Gender = Gender.Male,
                DateOfBirth = new DateOnly(1990, 1, 1),
                Address = "456 Client Ave",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                ClientProfile = new ClientProfile()
            };

            var result = await userManager.CreateAsync(clientUser, "Client@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(clientUser, "Client");
            }
        }
        else
        {
            clientUser.PhoneNumber = "01100000000";
            clientUser.Gender = Gender.Male;
            clientUser.DateOfBirth = new DateOnly(1990, 1, 1);
            clientUser.Address = "456 Client Ave";
            
            await userManager.UpdateAsync(clientUser);
        }
    }
}
