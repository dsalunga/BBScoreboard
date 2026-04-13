using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BBScoreboard.Infrastructure.Data;

public sealed class PostgresBbScoreboardDbContextFactory : IDesignTimeDbContextFactory<PostgresBbScoreboardDbContext>
{
    public PostgresBbScoreboardDbContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeDbContextConfiguration.CreateDbContextConfiguration();
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Host=localhost;Database=bbscoreboard;Username=postgres";
        }

        var optionsBuilder = new DbContextOptionsBuilder<PostgresBbScoreboardDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            builder => builder.MigrationsAssembly("BBScoreboard.Infrastructure"));

        return new PostgresBbScoreboardDbContext(optionsBuilder.Options);
    }

}

public sealed class SqlServerBbScoreboardDbContextFactory : IDesignTimeDbContextFactory<SqlServerBbScoreboardDbContext>
{
    public SqlServerBbScoreboardDbContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeDbContextConfiguration.CreateDbContextConfiguration();
        var connectionString = configuration.GetConnectionString("SqlServer");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Server=(localdb)\\mssqllocaldb;Database=BBScoreboard;Trusted_Connection=True;TrustServerCertificate=True";
        }

        var optionsBuilder = new DbContextOptionsBuilder<SqlServerBbScoreboardDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            builder => builder.MigrationsAssembly("BBScoreboard.Infrastructure"));

        return new SqlServerBbScoreboardDbContext(optionsBuilder.Options);
    }
}

internal static class DesignTimeDbContextConfiguration
{
    internal static IConfigurationRoot CreateDbContextConfiguration()
    {
        var current = Directory.GetCurrentDirectory();
        var webProject = Path.GetFullPath(Path.Combine(current, "..", "BBScoreboard.Web"));
        var basePath = Directory.Exists(webProject) ? webProject : current;

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
