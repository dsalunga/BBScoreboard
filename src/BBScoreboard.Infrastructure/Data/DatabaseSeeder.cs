using BBScoreboard.Domain.Enums;
using BBScoreboard.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BBScoreboard.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var config = services.GetRequiredService<IConfiguration>();
        var logger = services.GetService<ILoggerFactory>()?.CreateLogger("DatabaseSeeder");

        if (!config.GetValue("Database:AutoMigrate", true))
        {
            logger?.LogInformation("Database auto-migration/seeding is disabled by configuration.");
            return;
        }

        var provider = (config.GetValue<string>("Database:Provider") ?? "postgres").ToLowerInvariant();
        await MigrateAsync(provider, config);

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        await EnsureRoleExistsAsync(roleManager, "Admin");
        await EnsureRoleExistsAsync(roleManager, "Scorer");

        if (!await userManager.Users.AnyAsync())
        {
            var email = config["BootstrapAdmin:Email"] ?? config["DefaultUserEmail"];
            var password = config["BootstrapAdmin:Password"] ?? config["DefaultPassword"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger?.LogInformation("Bootstrap admin credentials are empty. Skipping admin account creation.");
                return;
            }

            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Access = AccessType.Admin,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(admin, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create bootstrap admin user: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to assign Admin role to bootstrap user: {errors}");
            }
        }
    }

    private static async Task EnsureRoleExistsAsync(RoleManager<IdentityRole<int>> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(new IdentityRole<int>(roleName));
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create role '{roleName}': {errors}");
            }
        }
    }

    private static async Task MigrateAsync(string provider, IConfiguration config)
    {
        switch (provider)
        {
            case "inmemory":
            {
                var options = new DbContextOptionsBuilder<BBScoreboardDbContext>()
                    .UseInMemoryDatabase("BBScoreboardTestDb")
                    .Options;
                await using var context = new BBScoreboardDbContext(options);
                await context.Database.EnsureCreatedAsync();
                break;
            }
            case "sqlserver":
            {
                var connection = config.GetConnectionString("SqlServer");
                if (string.IsNullOrWhiteSpace(connection))
                {
                    throw new InvalidOperationException("ConnectionStrings:SqlServer is required when Database:Provider is sqlserver.");
                }

                var options = new DbContextOptionsBuilder<SqlServerBbScoreboardDbContext>()
                    .UseSqlServer(connection, b => b.MigrationsAssembly("BBScoreboard.Infrastructure"))
                    .Options;
                await using var context = new SqlServerBbScoreboardDbContext(options);
                await context.Database.MigrateAsync();
                break;
            }
            default:
            {
                var connection = config.GetConnectionString("Postgres");
                if (string.IsNullOrWhiteSpace(connection))
                {
                    throw new InvalidOperationException("ConnectionStrings:Postgres is required when Database:Provider is postgres.");
                }

                var options = new DbContextOptionsBuilder<PostgresBbScoreboardDbContext>()
                    .UseNpgsql(connection, b => b.MigrationsAssembly("BBScoreboard.Infrastructure"))
                    .Options;
                await using var context = new PostgresBbScoreboardDbContext(options);
                await context.Database.MigrateAsync();
                break;
            }
        }
    }
}
