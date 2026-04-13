using BBScoreboard.Domain.Entities;
using BBScoreboard.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BBScoreboard.Tests;

public class DatabaseProviderIntegrationTests
{
    [Fact]
    [Trait("Category", "Database")]
    [Trait("Provider", "Postgres")]
    public async Task Postgres_MigrationsAndCrud_Work()
    {
        var baseConnection = Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(baseConnection))
        {
            return;
        }

        var dbName = $"bbscoreboard_it_{Guid.NewGuid():N}";
        var adminBuilder = new NpgsqlConnectionStringBuilder(baseConnection) { Database = "postgres" };
        await using var adminConnection = new NpgsqlConnection(adminBuilder.ConnectionString);
        await adminConnection.OpenAsync();

        await using (var createCommand = new NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", adminConnection))
        {
            await createCommand.ExecuteNonQueryAsync();
        }

        try
        {
            var testBuilder = new NpgsqlConnectionStringBuilder(baseConnection) { Database = dbName };
            var options = new DbContextOptionsBuilder<PostgresBbScoreboardDbContext>()
                .UseNpgsql(testBuilder.ConnectionString, builder => builder.MigrationsAssembly("BBScoreboard.Infrastructure"))
                .Options;

            await using var context = new PostgresBbScoreboardDbContext(options);
            await context.Database.MigrateAsync();

            context.UCSeasons.Add(new UCSeason { Name = "2026-27" });
            await context.SaveChangesAsync();

            var season = await context.UCSeasons.SingleAsync();
            Assert.Equal("2026-27", season.Name);
        }
        finally
        {
            await using var dropCommand = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{dbName}\" WITH (FORCE)", adminConnection);
            await dropCommand.ExecuteNonQueryAsync();
        }
    }

    [Fact]
    [Trait("Category", "Database")]
    [Trait("Provider", "SqlServer")]
    public async Task SqlServer_MigrationsAndCrud_Work()
    {
        var baseConnection = Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(baseConnection))
        {
            return;
        }

        var dbName = $"BBScoreboardIT_{Guid.NewGuid():N}";
        var adminBuilder = new SqlConnectionStringBuilder(baseConnection) { InitialCatalog = "master" };
        await using var adminConnection = new SqlConnection(adminBuilder.ConnectionString);
        await adminConnection.OpenAsync();

        await using (var createCommand = new SqlCommand($"CREATE DATABASE [{dbName}]", adminConnection))
        {
            await createCommand.ExecuteNonQueryAsync();
        }

        try
        {
            var testBuilder = new SqlConnectionStringBuilder(baseConnection) { InitialCatalog = dbName };
            var options = new DbContextOptionsBuilder<SqlServerBbScoreboardDbContext>()
                .UseSqlServer(testBuilder.ConnectionString, builder => builder.MigrationsAssembly("BBScoreboard.Infrastructure"))
                .Options;

            await using var context = new SqlServerBbScoreboardDbContext(options);
            await context.Database.MigrateAsync();

            context.UCTeams.Add(new UCTeam { Name = "Integration Team", TeamColor = "#FFFFFF", Active = true });
            await context.SaveChangesAsync();

            var team = await context.UCTeams.SingleAsync();
            Assert.Equal("Integration Team", team.Name);
        }
        finally
        {
            var dropSql = $@"
IF DB_ID(N'{dbName}') IS NOT NULL
BEGIN
    ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{dbName}];
END";
            await using var dropCommand = new SqlCommand(dropSql, adminConnection);
            await dropCommand.ExecuteNonQueryAsync();
        }
    }
}
