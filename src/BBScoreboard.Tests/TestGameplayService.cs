using BBScoreboard.Application;
using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Tests;

public sealed class TestGameplayService : IGameplayService
{
    public Task<GameplayModel> CreateAsync(int gameId) => throw new NotImplementedException();

    public Task<GameplayModel?> BuildGameplayModelAsync(int gameId) => Task.FromResult<GameplayModel?>(null);

    public Task UpdateTeamStatAsync(UCGameTeamStat stat) => Task.CompletedTask;

    public Task StartGameAsync(int gameId, IEnumerable<int> start1, IEnumerable<int> start2, int mins, bool autoStart) => Task.CompletedTask;

    public Task StartGameAsync(int gameId, List<int> starters, int mins, bool autoStart) => Task.CompletedTask;

    public Task ResetGameAsync(int gameId) => Task.CompletedTask;

    public Task<string> SendActionAsync(int gameId, int teamId, int playerId, int action, int arg, int recPlayerId)
        => Task.FromResult($"{{\"gameId\":{gameId},\"teamId\":{teamId},\"playerId\":{playerId},\"action\":{action},\"arg\":{arg},\"recPlayerId\":{recPlayerId}}}");

    public Task<int> UpdateActionAsync(int id, int mm, int ss) => Task.FromResult(id + mm + ss);

    public Task<int> UpdateTimerAsync(int gameId, int start, DateTime timeLeft, int tlMs, DateTime timerLastModified, int tlmMs)
        => Task.FromResult(gameId + start + tlMs + tlmMs);

    public Task<int> UpdateGameAsync(int gameId, int quarter, bool updateScores, UCGameTeamStat ts0, UCGameTeamStat ts1, bool updateTime, DateTime timeLeft, int tlMs, DateTime timerLastModified, int tlmMs)
        => Task.FromResult(gameId + quarter + (updateScores ? 1 : 0) + (updateTime ? 1 : 0));

    public Task<int> IsGameStartedAsync(int gameId) => Task.FromResult(gameId % 2);

    public Task<string> GetDeltaAsync(int gameId, DateTime lastUpdate)
        => Task.FromResult($"{{\"gameId\":{gameId},\"lastUpdate\":\"{lastUpdate:O}\"}}");

    public Task<string> GetDelta2Async(int gameId, DateTime lastUpdate, bool firstSync)
        => Task.FromResult($"{{\"gameId\":{gameId},\"firstSync\":{firstSync.ToString().ToLowerInvariant()}}}");
}
