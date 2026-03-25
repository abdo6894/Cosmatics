using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoveLens.Domain.Users;
using MoveLens.Domain.Users.Entities;
using MoveLens.Domain.Users.Enums;
using MoveLens.Domain.Users.ValueObjects;
using MoveLens.Infrastructure.Identity;

namespace MoveLens.Infrastructure.Persistence.Data.Initializer;

public sealed class AppDbContextInitializer(
    AppDbContext context,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ILogger<AppDbContextInitializer> logger)
{
    public async Task MigrateAsync(CancellationToken ct = default)
    {
        try
        {
            await context.Database.MigrateAsync(ct);
            logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        try
        {
            await SeedRolesAsync();
            await SeedUsersAsync(ct);
            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = ["Admin", "User"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Role '{Role}' created.", role);
            }
        }
    }

    private async Task SeedUsersAsync(CancellationToken ct)
    {
        await SeedAdminAsync(ct);
        await SeedSampleUsersAsync(ct);
    }

    private async Task SeedAdminAsync(CancellationToken ct)
    {
        const string adminEmail = "admin@movelens.com";
        const string adminPassword = "Admin@12345";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var appUser = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
        };

        var identityResult = await userManager.CreateAsync(appUser, adminPassword);
        if (!identityResult.Succeeded)
        {
            logger.LogError("Failed to create admin user: {Errors}",
                string.Join(", ", identityResult.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(appUser, "Admin");
        await userManager.AddToRoleAsync(appUser, "User");

        var preferences = UserPreferences.Create(
            moods: [OutingMood.Family, OutingMood.Cultural],
            language: PreferredLanguage.Arabic,
            maxBudget: 500m,
            governorates: ["Cairo", "Giza"]);

        var userResult = User.Create("MoveLens Admin", appUser.Id, preferences);
        if (!userResult.IsSuccess)
        {
            logger.LogError("Failed to create admin domain user.");
            return;
        }

        await context.Users.AddAsync(userResult.Value, ct);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Admin user seeded.");
    }

    private async Task SeedSampleUsersAsync(CancellationToken ct)
    {
        var sampleUsers = new[]
        {
            new { FullName = "Ahmed Hassan", Email = "ahmed@movelens.com", Governorate = "Cairo",       Budget = 300m },
            new { FullName = "Sara Mohamed", Email = "sara@movelens.com",  Governorate = "Alexandria",  Budget = 200m },
            new { FullName = "Omar Khaled",  Email = "omar@movelens.com",  Governorate = "Giza",        Budget = 150m },
        };

        foreach (var sample in sampleUsers)
        {
            if (await userManager.FindByEmailAsync(sample.Email) is not null)
                continue;

            var appUser = new AppUser
            {
                UserName = sample.Email,
                Email = sample.Email,
                EmailConfirmed = true,
            };

            var identityResult = await userManager.CreateAsync(appUser, "User@12345");
            if (!identityResult.Succeeded)
            {
                logger.LogError("Failed to create user {Email}: {Errors}",
                    sample.Email,
                    string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                continue;
            }

            await userManager.AddToRoleAsync(appUser, "User");

            var preferences = UserPreferences.Create(
                moods: [OutingMood.Family],
                language: PreferredLanguage.Arabic,
                maxBudget: sample.Budget,
                governorates: [sample.Governorate]);

            var userResult = User.Create(sample.FullName, appUser.Id, preferences);
            if (!userResult.IsSuccess) continue;

            await context.Users.AddAsync(userResult.Value, ct);
            logger.LogInformation("Sample user '{FullName}' seeded.", sample.FullName);
        }

        await context.SaveChangesAsync(ct);
    }
}


public static class AppDbContextInitializerExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initializer = scope.ServiceProvider
            .GetRequiredService<AppDbContextInitializer>();

        await initializer.MigrateAsync();
        await initializer.SeedAsync();
    }
}