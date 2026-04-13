using BBScoreboard.Domain.Entities;
using BBScoreboard.Infrastructure.Data;
using BBScoreboard.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace BBScoreboard.Tests;

public class ServiceIntegrationTests : IDisposable
{
    private readonly BBScoreboardDbContext _db;

    public ServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<BBScoreboardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new BBScoreboardDbContext(options);
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }

    // --- SeasonService ---

    [Fact]
    public async Task SeasonService_CreateAndGetAll()
    {
        var svc = new SeasonService(_db);

        var s1 = await svc.CreateAsync("2024-25");
        var s2 = await svc.CreateAsync("2023-24");

        var all = await svc.GetAllAsync();
        Assert.Equal(2, all.Count);
        Assert.Equal("2023-24", all[0].Name); // Sorted by name
        Assert.Equal("2024-25", all[1].Name);
    }

    [Fact]
    public async Task SeasonService_GetById()
    {
        var svc = new SeasonService(_db);
        var created = await svc.CreateAsync("Test Season");

        var found = await svc.GetByIdAsync(created.Id);
        Assert.NotNull(found);
        Assert.Equal("Test Season", found.Name);
    }

    [Fact]
    public async Task SeasonService_Update()
    {
        var svc = new SeasonService(_db);
        var created = await svc.CreateAsync("Old Name");

        await svc.UpdateAsync(created.Id, "New Name");

        var updated = await svc.GetByIdAsync(created.Id);
        Assert.Equal("New Name", updated!.Name);
    }

    [Fact]
    public async Task SeasonService_Delete_CascadesGames()
    {
        var svc = new SeasonService(_db);
        var season = await svc.CreateAsync("ToDelete");

        _db.UCGames.Add(new UCGame { SeasonId = season.Id, Team1 = 1, Team2 = 2 });
        await _db.SaveChangesAsync();

        await svc.DeleteAsync(season.Id);

        Assert.Empty(await _db.UCSeasons.ToListAsync());
        Assert.Empty(await _db.UCGames.ToListAsync());
    }

    // --- TeamService ---

    [Fact]
    public async Task TeamService_CreateAndGetAll()
    {
        var svc = new TeamService(_db);

        await svc.CreateAsync("Zebras", "#000000", true);
        await svc.CreateAsync("Alphas", "#FFFFFF", true);

        var all = await svc.GetAllAsync();
        Assert.Equal(2, all.Count);
        Assert.Equal("Alphas", all[0].Name); // Sorted
    }

    [Fact]
    public async Task TeamService_Update()
    {
        var svc = new TeamService(_db);
        var team = await svc.CreateAsync("Old", "#000", true);

        await svc.UpdateAsync(team.Id, "New", "#FFF", false);

        var updated = await svc.GetByIdAsync(team.Id);
        Assert.NotNull(updated);
        Assert.Equal("New", updated.Name);
        Assert.Equal("#FFF", updated.TeamColor);
        Assert.False(updated.Active);
    }

    [Fact]
    public async Task TeamService_Delete_CascadesPlayers()
    {
        var svc = new TeamService(_db);
        var team = await svc.CreateAsync("ToDelete", "#000", true);

        _db.UCPlayers.Add(new UCPlayer { TeamId = team.Id, FirstName = "John", LastName = "Doe", PlayerNumber = 1 });
        await _db.SaveChangesAsync();

        await svc.DeleteAsync(team.Id);

        Assert.Empty(await _db.UCTeams.ToListAsync());
        Assert.Empty(await _db.UCPlayers.ToListAsync());
    }

    // --- PlayerService ---

    [Fact]
    public async Task PlayerService_CreateAndGetByTeam()
    {
        var svc = new PlayerService(_db);

        _db.UCTeams.Add(new UCTeam { Id = 10, Name = "Team A" });
        await _db.SaveChangesAsync();

        await svc.CreateAsync("Jane", "Smith", 23, 1, 10, true);
        await svc.CreateAsync("John", "Doe", 10, 2, 10, true);

        var players = await svc.GetByTeamAsync(10);
        Assert.Equal(2, players.Count);
    }

    [Fact]
    public async Task PlayerService_Update()
    {
        var svc = new PlayerService(_db);
        var player = await svc.CreateAsync("Old", "Name", 0, 1, 1, true);

        await svc.UpdateAsync(player.Id, "New", "Name", 99, 1, false);

        var updated = await svc.GetByIdAsync(player.Id);
        Assert.NotNull(updated);
        Assert.Equal("New", updated.FirstName);
        Assert.Equal(99, updated.PlayerNumber);
        Assert.False(updated.Active);
    }

    // --- GameService ---

    [Fact]
    public async Task GameService_CreateAndGetBySeason()
    {
        var svc = new GameService(_db);
        var season1 = await new SeasonService(_db).CreateAsync("S1");
        var season2 = await new SeasonService(_db).CreateAsync("S2");
        var t1 = await new TeamService(_db).CreateAsync("Team 1", "#111111", true);
        var t2 = await new TeamService(_db).CreateAsync("Team 2", "#222222", true);
        var t3 = await new TeamService(_db).CreateAsync("Team 3", "#333333", true);
        var t4 = await new TeamService(_db).CreateAsync("Team 4", "#444444", true);

        var now = DateTime.UtcNow.Date;
        await svc.CreateAsync(season1.Id, 1, t1.Id, t2.Id, now, "Venue A");
        await svc.CreateAsync(season1.Id, 2, t3.Id, t4.Id, now, "Venue B");
        await svc.CreateAsync(season2.Id, 1, t1.Id, t3.Id, now, "Venue C");

        var games = await svc.GetBySeasonAsync(season1.Id);
        Assert.Equal(2, games.Count);
    }

    [Fact]
    public async Task GameService_GetById()
    {
        var svc = new GameService(_db);
        var season = await new SeasonService(_db).CreateAsync("S1");
        var teamA = await new TeamService(_db).CreateAsync("Team A", "#111111", true);
        var teamB = await new TeamService(_db).CreateAsync("Team B", "#222222", true);
        var game = await svc.CreateAsync(season.Id, 1, teamA.Id, teamB.Id, DateTime.UtcNow, "Final");

        var found = await svc.GetByIdAsync(game.Id);
        Assert.NotNull(found);
        Assert.Equal(teamA.Id, found.Team1);
        Assert.Equal(teamB.Id, found.Team2);
    }

    [Fact]
    public async Task GameService_Create_RejectsSameTeams()
    {
        var season = await new SeasonService(_db).CreateAsync("S1");
        var team = await new TeamService(_db).CreateAsync("Team A", "#111111", true);
        var svc = new GameService(_db);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateAsync(season.Id, 1, team.Id, team.Id, DateTime.UtcNow, "Arena"));

        Assert.Equal("Team 1 and Team 2 must be different.", ex.Message);
    }

    [Fact]
    public async Task GameService_Create_NormalizesGameDateToUtc()
    {
        var season = await new SeasonService(_db).CreateAsync("S1");
        var teamA = await new TeamService(_db).CreateAsync("Team A", "#111111", true);
        var teamB = await new TeamService(_db).CreateAsync("Team B", "#222222", true);
        var svc = new GameService(_db);
        var localDate = new DateTime(2026, 4, 13); // Unspecified kind by default

        var game = await svc.CreateAsync(season.Id, 1, teamA.Id, teamB.Id, localDate, "Arena");

        Assert.Equal(DateTimeKind.Utc, game.GameDate.Kind);
        Assert.Equal(localDate.Date, game.GameDate.Date);
    }

    // --- AppConfigService ---

    [Fact]
    public async Task AppConfigService_GetAndSet()
    {
        var svc = new AppConfigService(_db);

        await svc.SetValueAsync("TestKey", "TestValue");

        var value = await svc.GetValueAsync("TestKey");
        Assert.Equal("TestValue", value);
    }

    [Fact]
    public async Task AppConfigService_GetBool_ReturnsFalseIfMissing()
    {
        var svc = new AppConfigService(_db);

        Assert.False(await svc.GetBoolAsync("NonExistent"));
    }

    [Fact]
    public async Task AppConfigService_GetBool_ParsesTrueValues()
    {
        var svc = new AppConfigService(_db);

        await svc.SetValueAsync("Flag", "true");
        Assert.True(await svc.GetBoolAsync("Flag"));

        await svc.SetValueAsync("Flag", "false");
        Assert.False(await svc.GetBoolAsync("Flag"));
    }
}
