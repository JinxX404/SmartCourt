using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SmartCourt.Common.Enums;
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
        await SeedMarketplaceLawyersAsync(userManager);
    }

    private static async Task SeedMarketplaceLawyersAsync(UserManager<ApplicationUser> userManager)
    {
        var lawyersToSeed = new List<(string Email, string Name, string Bio, LawyerLevel Level, Specialization Specialization)>
        {
            ("ahmed.mansour@smartcourt.com", "أحمد منصور", "محامي نقض خبير في القضايا الجنائية والتجارية بخبرة تتجاوز 20 عاماً.", LawyerLevel.CassationCourt, Specialization.CriminalLaw),
            ("sara.ali@smartcourt.com", "سارة علي", "محامية استئناف متخصصة في قضايا الأسرة والأحوال الشخصية.", LawyerLevel.AppealCourt, Specialization.FamilyLaw),
            ("mohamed.hassan@smartcourt.com", "محمد حسن", "محامي ابتدائي مهتم بقضايا الشركات وصياغة العقود التجارية.", LawyerLevel.PrimaryCourt, Specialization.CorporateLaw),
            ("fatma.kamal@smartcourt.com", "فاطمة كمال", "مستشارة قانونية ذات خبرة واسعة في الملكية الفكرية وتسجيل العلامات التجارية.", LawyerLevel.CassationCourt, Specialization.IntellectualProperty),
            ("mahmoud.tarek@smartcourt.com", "محمود طارق", "محامي جدول عام طموح يعمل في القضايا المدنية والمنازعات الإيجارية.", LawyerLevel.GeneralRegistration, Specialization.CivilLaw),
            ("youssef.ibrahim@smartcourt.com", "يوسف إبراهيم", "خبير في قضايا الجرائم الإلكترونية والابتزاز المالي.", LawyerLevel.CassationCourt, Specialization.Cybercrimes),
            ("nada.salem@smartcourt.com", "ندى سالم", "محامية متخصصة في قضايا العمل والعمال وصياغة لوائح الشركات.", LawyerLevel.AppealCourt, Specialization.LaborLaw),
            ("omar.farouk@smartcourt.com", "عمر فاروق", "محامي متخصص في القضايا العقارية وتسجيل الأراضي والعقارات.", LawyerLevel.PrimaryCourt, Specialization.RealEstateAndPropertyRegistration),
            ("laila.mostafa@smartcourt.com", "ليلى مصطفى", "محامية متمرسة في القضايا الإدارية ومجلس الدولة.", LawyerLevel.CassationCourt, Specialization.AdministrativeAndStateCouncilLaw),
            ("khaled.yassin@smartcourt.com", "خالد ياسين", "محامي متخصص في قضايا الضرائب والمنازعات المالية.", LawyerLevel.CassationCourt, Specialization.TaxLaw),
            ("mona.samir@smartcourt.com", "منى سمير", "محامية متخصصة في قضايا التعويضات وحوادث الطرق.", LawyerLevel.AppealCourt, Specialization.CivilLaw),
            ("hany.ramzy@smartcourt.com", "هاني رمزي", "محامي شركات وتأسيس منشآت أعمال دولية ومحلية.", LawyerLevel.CassationCourt, Specialization.CorporateLaw),
            ("dina.magdy@smartcourt.com", "دينا مجدي", "باحثة قانونية ومحامية تحت التمرين في القضايا المدنية.", LawyerLevel.GeneralRegistration, Specialization.CivilLaw),
            ("tarek.adel@smartcourt.com", "طارق عادل", "محامي نقض خبير في المنازعات الجمركية وقضايا التهرب.", LawyerLevel.CassationCourt, Specialization.CustomsLaw),
            ("samir.said@smartcourt.com", "سمير سعيد", "مستشار قانوني لعدد من البنوك وشركات التمويل.", LawyerLevel.CassationCourt, Specialization.BankingAndFinance),
            ("wael.zaky@smartcourt.com", "وائل زكي", "محامي استئناف خبير في قضايا التحكيم التجاري والدولي.", LawyerLevel.AppealCourt, Specialization.CorporateLaw),
            ("reem.hassan@smartcourt.com", "ريم حسن", "محامية متخصصة في تأسيس الشركات الأجنبية وصياغة عقود الفرنشايز.", LawyerLevel.AppealCourt, Specialization.CorporateLaw),
            ("amr.diab@smartcourt.com", "عمرو دياب", "خبير في قضايا الملكية الفكرية وبراءات الاختراع.", LawyerLevel.CassationCourt, Specialization.IntellectualProperty),
            ("shaimaa.ali@smartcourt.com", "شيماء علي", "محامية متخصصة في المنازعات العمالية والتأمينات الاجتماعية.", LawyerLevel.AppealCourt, Specialization.LaborLaw),
            ("hassan.kamal@smartcourt.com", "حسن كمال", "مستشار قانوني خبير في صياغة العقود العقارية والمقاولات.", LawyerLevel.CassationCourt, Specialization.RealEstateAndPropertyRegistration)
        };

        for (int i = 1; i <= 30; i++)
        {
            var level = (LawyerLevel)((i % 4) + 1);
            var spec = (Specialization)(i % 21);
            lawyersToSeed.Add(($"lawyer{i}@smartcourt.com", $"المحامي {i}", $"نبذة عن المحامي {i} وهو متخصص ويمتلك خبرة واسعة في قضايا متعددة.", level, spec));
        }

        int counter = 100;
        foreach (var l in lawyersToSeed)
        {
            var isMale = counter % 2 == 0;
            var pictureUrl = isMale 
                ? $"https://randomuser.me/api/portraits/men/{counter % 100}.jpg" 
                : $"https://randomuser.me/api/portraits/women/{counter % 100}.jpg";

            var existingUser = await userManager.FindByEmailAsync(l.Email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = l.Email,
                    Email = l.Email,
                    FullName = l.Name,
                    PhoneNumber = $"01000000{counter}",
                    NationalNumber = $"29001011234{counter}",
                    Gender = isMale ? Gender.Male : Gender.Female,
                    DateOfBirth = new DateOnly(1980 + (counter % 10), 1, 1),
                    Address = "القاهرة، مصر",
                    Status = UserStatus.Active,
                    EmailConfirmed = true,
                    ProfilePictureUrl = pictureUrl,
                    LawyerProfile = new LawyerProfile
                    {
                        Bio = l.Bio,
                        Level = l.Level,
                        Specializations = new List<LawyerSpecialization>
                        {
                            new LawyerSpecialization { Specialization = l.Specialization, YearsOfExperience = 5, CasesHandled = 10 }
                        },
                        AverageRating = 4.0m + (decimal)(counter % 10) / 10m,
                        IsAvailable = true
                    }
                };

                var res = await userManager.CreateAsync(user, "Lawyer@123");
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Lawyer");
                }
            }
            else
            {
                // Update existing users with profile pictures so we don't have to wipe the DB
                existingUser.ProfilePictureUrl = pictureUrl;
                await userManager.UpdateAsync(existingUser);
            }
            counter++;
        }
    }
}
