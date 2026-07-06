using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PatentAttorneyAdmin.Models;

namespace PatentAttorneyAdmin.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        const string adminRole = "Admin";
        const string clientRole = "Client";

        if (!await roleManager.RoleExistsAsync(adminRole))
            await roleManager.CreateAsync(new IdentityRole(adminRole));

        if (!await roleManager.RoleExistsAsync(clientRole))
            await roleManager.CreateAsync(new IdentityRole(clientRole));

        const string adminEmail = "admin@local.dev";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "System",
                PhoneNumber = "+992000000000",
                Status = UserStatus.Active,
                RegisteredAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, adminRole);
        }

        await SeedServicesAsync(context);
    }


    private static async Task SeedServicesAsync(ApplicationDbContext context)
    {
        var services = AllServices();

        if (!await context.ServiceCategories.AnyAsync())
        {
            context.ServiceCategories.AddRange(services);
            await context.SaveChangesAsync();
            return;
        }

        foreach (var service in services)
        {
            var existing = await context.ServiceCategories
                .FirstOrDefaultAsync(s => s.ServiceCode == service.ServiceCode);

            if (existing == null)
            {
                context.ServiceCategories.Add(service);
            }
            else
            {
                existing.TitleRu = service.TitleRu;
                existing.TitleTj = service.TitleTj;
                existing.DescriptionRu = service.DescriptionRu;
                existing.DescriptionTj = service.DescriptionTj;
                existing.IconClass = service.IconClass;
                existing.SortOrder = service.SortOrder;
                existing.IsActive = true;
            }
        }

        var placeholders = await context.ServiceCategories
            .Where(s => s.ServiceCode == null || s.ServiceCode == "")
            .ToListAsync();

        foreach (var placeholder in placeholders)
            placeholder.IsActive = false;

        await context.SaveChangesAsync();
    }

    private static ServiceCategory[] AllServices() => new[]
    {
        new ServiceCategory
        {
            ServiceCode = ServiceCodes.HomsCourse,
            IconClass = "bi-mortarboard",
            TitleRu = "4.1. Обучение в кафедре ХОМС (Основы интеллектуальной собственности)",
            TitleTj = "4.1. Таҳсил дар кафедраи ҲОМС (Асосҳои моликияти зеҳнӣ)",
            DescriptionRu = "Заявление на обучение в курсе «Основы интеллектуальной собственности». По окончании выдаётся сертификат.",
            DescriptionTj = "Ариза барои таҳсил дар курси «Асосҳои моликияти зеҳнӣ». Пас аз анҷом сертификат дода мешавад.",
            SortOrder = 1,
            IsActive = true
        },
        new ServiceCategory
        {
            ServiceCode = ServiceCodes.PatentRepCertification,
            IconClass = "bi-person-badge",
            TitleRu = "4.2. Аттестация патентного представителя (доверитель)",
            TitleTj = "4.2. Муроҷиат барои бақайдгирии намояндагони патентии ҶТ (довталаб)",
            DescriptionRu = "Заявление на прохождение аттестации для регистрации в качестве патентного представителя РТ.",
            DescriptionTj = "Ариза барои гузаштан аз аттестатсия ба Директори Идораи патентӣ.",
            SortOrder = 2,
            IsActive = true
        },
        new ServiceCategory
        {
            ServiceCode = ServiceCodes.RegisterCertificate,
            IconClass = "bi-patch-check",
            TitleRu = "4.3. Регистрация в Реестре и получение Свидетельства (доверитель)",
            TitleTj = "4.3. Муроҷиат барои бақайдгирии дар Феҳрист ва гирифтани Шаҳодатнома (довталаб)",
            DescriptionRu = "Заявление для лиц, успешно сдавших квалификационный экзамен, на получение свидетельства патентного поверенного.",
            DescriptionTj = "Ариза барои гирифтани шаҳодатномаи патентии вакил пас аз гузаронидани имтиҳони квалификатсионӣ.",
            SortOrder = 3,
            IsActive = true
        },
        new ServiceCategory
        {
            ServiceCode = ServiceCodes.VrisLicense,
            IconClass = "bi-file-earmark-ruled",
            TitleRu = "4.4. Получение лицензии (ВРИС)",
            TitleTj = "4.4. Ариза барои гирифтани иҷозатнома (ВРИС)",
            DescriptionRu = "Заявление на получение лицензии ВРИС. Требуются основные и дополнительные документы.",
            DescriptionTj = "Ариза барои гирифтани иҷозатнома (ВРИС). Ҳуҷҷатҳои асосӣ ва иловагӣ заруранд.",
            SortOrder = 4,
            IsActive = true
        }
    };
}
