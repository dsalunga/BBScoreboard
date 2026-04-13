using BBScoreboard.Application.Interfaces;
using BBScoreboard.Infrastructure.Data;
using BBScoreboard.Infrastructure.Identity;
using BBScoreboard.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBScoreboard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>("Database:Provider")?.ToLowerInvariant() ?? "postgres";

        services.AddDbContext<BBScoreboardDbContext>(options =>
        {
            switch (provider)
            {
                case "sqlserver":
                    options.UseSqlServer(
                        configuration.GetConnectionString("SqlServer"),
                        b => b.MigrationsAssembly("BBScoreboard.Infrastructure"));
                    break;
                default: // postgres
                    options.UseNpgsql(
                        configuration.GetConnectionString("Postgres"),
                        b => b.MigrationsAssembly("BBScoreboard.Infrastructure"));
                    break;
            }
        });

        services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.Lockout.MaxFailedAccessAttempts = 4;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(60);
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<BBScoreboardDbContext>()
        .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Login";
            options.LogoutPath = "/Login?Action=Logoff";
            options.AccessDeniedPath = "/Login";
            options.ExpireTimeSpan = TimeSpan.FromHours(5);
        });

        // Application services
        services.AddScoped<IAppConfigService, AppConfigService>();
        services.AddScoped<IGameplayService, GameplayService>();
        services.AddScoped<ISeasonService, SeasonService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IGameService, GameService>();

        return services;
    }
}
