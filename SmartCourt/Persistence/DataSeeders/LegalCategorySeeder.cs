using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;

namespace SmartCourt.Persistence.DataSeeders;

public static class LegalCategorySeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.LegalCategories.AnyAsync())
        {
            return;
        }

        var categories = new List<LegalCategory>
        {
            new LegalCategory
            {
                Id = Guid.NewGuid(),
                Name = "القانون المدني",
                Description = "العلاقات الفردية والمؤسسية العامة، بما في ذلك العقود والالتزامات والتعويضات والمنازعات المدنية.",
                Specializations = new List<LegalSpecialization>
                {
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "العقود" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "التعويضات والمسؤولية التقصيرية" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "تحصيل الديون" }
                }
            },
            new LegalCategory
            {
                Id = Guid.NewGuid(),
                Name = "القانون الجنائي",
                Description = "الدفاع أو تمثيل الموكلين في القضايا الجنائية، من المخالفات البسيطة إلى الجنايات الخطيرة.",
                Specializations = new List<LegalSpecialization>
                {
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "الجرائم المالية والاختلاس" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "جرائم المخدرات" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "جرائم الأموال العامة" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "الجنايات والجنح" }
                }
            },
            new LegalCategory
            {
                Id = Guid.NewGuid(),
                Name = "القانون التجاري والشركات",
                Description = "العلاقات التجارية، تأسيس الشركات، حوكمة الشركات، وإعادة الهيكلة.",
                Specializations = new List<LegalSpecialization>
                {
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "تأسيس الشركات" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "الاندماج والاستحواذ" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "العقود التجارية" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "حوكمة الشركات" }
                }
            },
            new LegalCategory
            {
                Id = Guid.NewGuid(),
                Name = "قانون الأسرة والأحوال الشخصية",
                Description = "مسائل الأحوال الشخصية مثل الزواج والطلاق وحضانة الأطفال والمواريث.",
                Specializations = new List<LegalSpecialization>
                {
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "الزواج والطلاق" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "حضانة الأطفال" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "المواريث" }
                }
            },
            new LegalCategory
            {
                Id = Guid.NewGuid(),
                Name = "القانون العقاري",
                Description = "المعاملات العقارية، التسجيل العقاري، عقود الإيجار، وتسوية النزاعات العقارية.",
                Specializations = new List<LegalSpecialization>
                {
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "المعاملات العقارية والشهر العقاري" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "عقود الإيجار" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "النزاعات العقارية" }
                }
            },
            new LegalCategory
            {
                Id = Guid.NewGuid(),
                Name = "القانون الإداري",
                Description = "المسائل القانونية التي تشمل الهيئات الحكومية والإدارة العامة.",
                Specializations = new List<LegalSpecialization>
                {
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "العقود الحكومية" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "منازعات مجلس الدولة" }
                }
            },
            new LegalCategory
            {
                Id = Guid.NewGuid(),
                Name = "قانون العمل",
                Description = "علاقات أصحاب العمل والموظفين، نزاعات العمل، والامتثال للوائح العمل.",
                Specializations = new List<LegalSpecialization>
                {
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "منازعات العمل" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "الفصل التعسفي" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "الامتثال لقوانين العمل" }
                }
            },
            new LegalCategory
            {
                Id = Guid.NewGuid(),
                Name = "المجالات المتخصصة والناشئة",
                Description = "مجالات الممارسة المتخصصة بما في ذلك الملكية الفكرية، قانون الإنترنت، وتسوية المنازعات.",
                Specializations = new List<LegalSpecialization>
                {
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "تسوية المنازعات والتحكيم" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "الملكية الفكرية" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "قانون الإنترنت والجرائم الإلكترونية" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "البنوك والتمويل والتكنولوجيا المالية" },
                    new LegalSpecialization { Id = Guid.NewGuid(), Name = "قانون الهجرة" }
                }
            }
        };

        await context.LegalCategories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }
}
