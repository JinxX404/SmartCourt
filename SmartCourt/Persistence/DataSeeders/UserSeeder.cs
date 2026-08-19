using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Entities;
using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Persistence.DataSeeders
{
    public static class UserSeeder
    {
        public static async Task SeedTestLawyerAsync(
            UserManager<ApplicationUser> userManager)
        {
            const string email = "lawyer@test.com";

            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser is not null)
                return;

            var user = new ApplicationUser
            {
                UserName = "lawyer",
                Email = email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "12345678");

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(Environment.NewLine,
                    result.Errors.Select(e => e.Description)));
            }

            await userManager.AddToRoleAsync(user, "Lawyer");

            Console.WriteLine($"Test Lawyer Id = {user.Id}");
        }

        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
            "Admin",
            "Lawyer",
            "Client"
        };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedTestClientAsync(
            UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            const string email = "client@test.com";

            if (await userManager.Users.AnyAsync(x => x.Email == email))
                return;

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FullName = "Test Client",
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                NationalNumber = "29801011234567",
                Status = UserStatus.Active
            };

            var result = await userManager.CreateAsync(user, "Client@123");

            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Failed to create test client: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            var clientProfile = new ClientProfile
            {
                UserId = user.Id,
                User = user
            };

            user.ClientProfile = clientProfile;

            context.ClientProfile.Add(clientProfile);

            await context.SaveChangesAsync();
        }
        public static async Task SeedTestAdminAsync(UserManager<ApplicationUser> userManager)
        {
            const string email = "mahmoud@admin.com";

            if (await userManager.Users.AnyAsync(x => x.Email == email))
                return;

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FullName = "Admin Mahmoud",
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Status = UserStatus.Active
            };

            var result = await userManager.CreateAsync(user, "Mn12345678");

            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}
