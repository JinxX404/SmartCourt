using Microsoft.AspNetCore.Identity;
using SmartCourt.Entities;

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
    }
}
