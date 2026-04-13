using BBScoreboard.Domain.Enums;
using BBScoreboard.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBScoreboard.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<BBScoreboardDbContext>();
        await context.Database.MigrateAsync();

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var config = services.GetRequiredService<IConfiguration>();

        if (!await userManager.Users.AnyAsync())
        {
            var email = config["DefaultUserEmail"] ?? "admin@bbscoreboard.local";
            var password = config["DefaultPassword"] ?? "Changeme1!";

            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Access = AccessType.Admin,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, password);
        }
    }
}
