using DTS.Domain.Entities;
using DTS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DTS.Infrastructure.SeedData;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");

        try
        {
            await context.Database.MigrateAsync();
            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);
            await SeedSampleDataAsync(context);
            logger.LogInformation("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Administrator", "ProjectManager", "Tester", "Developer", "Client" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<AppUser> userManager)
    {
        const string adminEmail = "admin@dts.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                PhoneNumber = "+1-555-0100",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }
    }

    private static async Task SeedSampleDataAsync(DbContext context)
    {
        if (!await context.Set<Project>().AnyAsync())
        {
            var projects = new List<Project>
            {
                new()
                {
                    Name = "E-Commerce Platform",
                    Description = "Online shopping platform with payment integration",
                    StartDate = DateTime.UtcNow.AddMonths(-3),
                    EndDate = DateTime.UtcNow.AddMonths(6),
                    Status = Domain.Enums.ProjectStatus.Active,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Name = "Mobile Banking App",
                    Description = "Secure mobile banking application",
                    StartDate = DateTime.UtcNow.AddMonths(-1),
                    EndDate = DateTime.UtcNow.AddMonths(8),
                    Status = Domain.Enums.ProjectStatus.Active,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Name = "CRM System",
                    Description = "Customer relationship management system",
                    StartDate = DateTime.UtcNow.AddMonths(-6),
                    EndDate = DateTime.UtcNow.AddMonths(2),
                    Status = Domain.Enums.ProjectStatus.Active,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Set<Project>().AddRangeAsync(projects);
            await context.SaveChangesAsync();
        }
    }
}
