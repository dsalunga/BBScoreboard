using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Application.Interfaces;

public interface IGameplayService
{
    Task<GameplayModel> CreateAsync(int gameId);
    Task<GameplayModel?> BuildGameplayModelAsync(int gameId);
    Task UpdateTeamStatAsync(UCGameTeamStat stat);
    Task StartGameAsync(int gameId, IEnumerable<int> start1, IEnumerable<int> start2, int mins, bool autoStart);
    Task StartGameAsync(int gameId, List<int> starters, int mins, bool autoStart);
    Task ResetGameAsync(int gameId);
    Task<string> SendActionAsync(int gameId, int teamId, int playerId, int action, int arg, int recPlayerId);
    Task<int> UpdateActionAsync(int id, int mm, int ss);
    Task<int> UpdateTimerAsync(int gameId, int start, DateTime timeLeft, int tlMs, DateTime timerLastModified, int tlmMs);
    Task<int> UpdateGameAsync(int gameId, int quarter, bool updateScores, UCGameTeamStat ts0, UCGameTeamStat ts1, bool updateTime, DateTime timeLeft, int tlMs, DateTime timerLastModified, int tlmMs);
    Task<int> IsGameStartedAsync(int gameId);
    Task<string> GetDeltaAsync(int gameId, DateTime lastUpdate);
    Task<string> GetDelta2Async(int gameId, DateTime lastUpdate, bool firstSync);
}
