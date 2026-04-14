using BBScoreboard.Domain.Entities;
using BBScoreboard.Infrastructure.Data;
using BBScoreboard.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace BBScoreboard.Tests;

public class GameplayParityIntegrationTests : IDisposable
{
    private readonly BBScoreboardDbContext _db;
    private readonly GameplayService _service;

    public GameplayParityIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<BBScoreboardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new BBScoreboardDbContext(options);
        _db.Database.EnsureCreated();
        _service = new GameplayService(_db, NullLogger<GameplayService>.Instance);
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task UpdateAction_ToggleUndoRedo_UpdatesScoreAndStatus()
    {
        var seeded = await SeedGameWithPlayersAsync();
        await _service.StartGameAsync(seeded.Game.Id, seeded.Start1, seeded.Start2, mins: 20, autoStart: false);

        await _service.SendActionAsync(seeded.Game.Id, seeded.Home.Id, seeded.HomeStarterId, action: 2, arg: 2, recPlayerId: -1);

        var action = await _db.UCGameplayActions.SingleAsync(a => a.GameId == seeded.Game.Id);
        var teamStat = await _db.UCGameTeamStats.SingleAsync(t => t.GameId == seeded.Game.Id && t.TeamId == seeded.Home.Id);
        Assert.Equal(1, action.Status);
        Assert.Equal(2, teamStat.Q1);

        await _service.UpdateActionAsync(action.Id, mm: -1, ss: 0);
        await _db.Entry(action).ReloadAsync();
        await _db.Entry(teamStat).ReloadAsync();
        Assert.Equal(0, action.Status);
        Assert.Equal(0, teamStat.Q1);

        await _service.UpdateActionAsync(action.Id, mm: -1, ss: 0);
        await _db.Entry(action).ReloadAsync();
        await _db.Entry(teamStat).ReloadAsync();
        Assert.Equal(1, action.Status);
        Assert.Equal(2, teamStat.Q1);
    }

    [Fact]
    public async Task UpdateTimer_ConvertsToUtcAndPersistsMilliseconds()
    {
        var seeded = await SeedGameWithPlayersAsync();
        var timeLeft = new DateTime(2026, 4, 14, 0, 5, 0, DateTimeKind.Unspecified);
        var modified = new DateTime(2026, 4, 14, 0, 6, 0, DateTimeKind.Unspecified);

        await _service.UpdateTimerAsync(seeded.Game.Id, start: 1, timeLeft, tlMs: 250, timerLastModified: modified, tlmMs: 125);

        var game = await _db.UCGames.SingleAsync(g => g.Id == seeded.Game.Id);
        Assert.True(game.IsTimeOn);
        Assert.Equal(DateTimeKind.Utc, game.TimeLeft.Kind);
        Assert.Equal(DateTimeKind.Utc, game.TimeLastModified.Kind);
        Assert.Equal(250, game.TimeLeft.Millisecond);
        Assert.Equal(125, game.TimeLastModified.Millisecond);
    }

    [Fact]
    public async Task GetDelta_ReturnCodes_FollowParityRules()
    {
        var seeded = await SeedGameWithPlayersAsync();
        var game = await _db.UCGames.SingleAsync(g => g.Id == seeded.Game.Id);

        game.LastUpdateForRefresh = DateTime.UtcNow;
        game.LastActivityDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var r2 = await _service.GetDeltaAsync(seeded.Game.Id, DateTime.UtcNow.AddMinutes(-10));
        Assert.Contains("\"return\": 2", r2);

        game.LastUpdateForRefresh = DateTime.UtcNow.AddHours(-1);
        game.LastActivityDate = DateTime.UtcNow.AddHours(-1);
        game.LastUpdate = DateTime.UtcNow.AddHours(-1);
        await _db.SaveChangesAsync();

        var r0 = await _service.GetDeltaAsync(seeded.Game.Id, DateTime.UtcNow);
        Assert.Contains("\"return\": 0", r0);

        var r1 = await _service.GetDelta2Async(seeded.Game.Id, DateTime.UtcNow, firstSync: true);
        Assert.Contains("\"return\": 1", r1);
        Assert.Contains("\"game\":", r1);
    }

    [Fact]
    public async Task GetDelta_IncludesActionPayloadForNewAndUpdatedActions()
    {
        var seeded = await SeedGameWithPlayersAsync();
        await _service.StartGameAsync(seeded.Game.Id, seeded.Start1, seeded.Start2, mins: 20, autoStart: false);

        var game = await _db.UCGames.SingleAsync(g => g.Id == seeded.Game.Id);
        game.LastUpdateForRefresh = DateTime.UtcNow.AddHours(-1);
        await _db.SaveChangesAsync();

        await _service.SendActionAsync(seeded.Game.Id, seeded.Home.Id, seeded.HomeStarterId, action: 3, arg: 3, recPlayerId: -1);
        var firstDelta = await _service.GetDeltaAsync(seeded.Game.Id, DateTime.UtcNow.AddMinutes(-10));
        Assert.Contains("\"actions\":", firstDelta);
        Assert.Contains("\"status\": -1", firstDelta);

        var action = await _db.UCGameplayActions.SingleAsync(a => a.GameId == seeded.Game.Id);
        await _service.UpdateActionAsync(action.Id, mm: -1, ss: 0);
        var updatedDelta = await _service.GetDeltaAsync(seeded.Game.Id, DateTime.UtcNow.AddMinutes(-10));
        Assert.Contains("\"status\":0", updatedDelta);
        Assert.Contains("\"time\":\"", updatedDelta);
    }

    [Fact]
    public async Task UpdateGame_QuarterOvertime_CreatesQuarterFourStats()
    {
        var seeded = await SeedGameWithPlayersAsync();
        await _service.UpdateGameAsync(
            seeded.Game.Id,
            quarter: 6,
            updateScores: false,
            ts0: new UCGameTeamStat(),
            ts1: new UCGameTeamStat(),
            updateTime: false,
            timeLeft: DateTime.UtcNow,
            tlMs: 0,
            timerLastModified: DateTime.UtcNow,
            tlmMs: 0);

        var game = await _db.UCGames.SingleAsync(g => g.Id == seeded.Game.Id);
        Assert.Equal(6, game.CurrentQuarter);

        var q4Count = await _db.UCGamePlayerStats.CountAsync(ps => ps.GameId == seeded.Game.Id && ps.Quarter == 4);
        Assert.True(q4Count >= 10);
    }

    private async Task<(UCGame Game, UCTeam Home, int HomeStarterId, List<int> Start1, List<int> Start2)> SeedGameWithPlayersAsync()
    {
        var season = await new SeasonService(_db).CreateAsync("Parity Season");
        var teamService = new TeamService(_db);
        var home = await teamService.CreateAsync("Home", "#111111", true);
        var away = await teamService.CreateAsync("Away", "#222222", true);

        var playerService = new PlayerService(_db);
        var start1 = new List<int>();
        var start2 = new List<int>();
        for (var i = 0; i < 5; i++)
        {
            var p1 = await playerService.CreateAsync("H", $"P{i + 1}", i + 1, positionId: 1, teamId: home.Id, active: true);
            var p2 = await playerService.CreateAsync("A", $"P{i + 1}", i + 1, positionId: 1, teamId: away.Id, active: true);
            start1.Add(p1.Id);
            start2.Add(p2.Id);
        }

        var game = await new GameService(_db).CreateAsync(
            season.Id,
            gameNumber: 1,
            team1: home.Id,
            team2: away.Id,
            gameDate: DateTime.UtcNow,
            venue: "Parity Arena");

        return (game, home, start1[0], start1, start2);
    }
}
