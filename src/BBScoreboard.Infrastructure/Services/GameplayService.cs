using System.Text;
using System.Text.Json;
using BBScoreboard.Application;
using BBScoreboard.Application.Interfaces;
using BBScoreboard.Application.Services;
using BBScoreboard.Domain;
using BBScoreboard.Domain.Entities;
using BBScoreboard.Domain.Enums;
using BBScoreboard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BBScoreboard.Infrastructure.Services;

public class GameplayService(BBScoreboardDbContext db) : IGameplayService
{
    public async Task<GameplayModel?> BuildGameplayModelAsync(int gameId)
    {
        var game = await db.UCGames.FindAsync(gameId);
        if (game == null) return null;
        return await CreateAsync(gameId);
    }

    public async Task UpdateTeamStatAsync(UCGameTeamStat stat)
    {
        db.UCGameTeamStats.Update(stat);
        await db.SaveChangesAsync();
    }

    public async Task StartGameAsync(int gameId, List<int> starters, int mins, bool autoStart)
    {
        var gp = await CreateAsync(gameId);
        // Split starters by team
        var team1Players = gp.TeamModels[0].Players.Select(p => p.Id).ToHashSet();
        var start1 = starters.Where(id => team1Players.Contains(id));
        var start2 = starters.Where(id => !team1Players.Contains(id));
        await StartGameInternalAsync(gp, start1, start2, mins, autoStart);
    }

    public async Task<GameplayModel> CreateAsync(int gameId)
    {
        var game = await db.UCGames.FindAsync(gameId)
            ?? throw new InvalidOperationException($"Game {gameId} not found");

        var season = await db.UCSeasons.FindAsync(game.SeasonId);
        var team1 = await db.UCTeams.SingleOrDefaultAsync(t => t.Id == game.Team1);
        var team2 = await db.UCTeams.SingleOrDefaultAsync(t => t.Id == game.Team2);

        var model = new GameplayModel
        {
            GameId = gameId,
            Game = game,
            Season = season
        };

        foreach (var team in new[] { team1, team2 })
        {
            if (team == null) continue;
            var tm = await InitializeTeamModelAsync(model, team);
            model.TeamModels.Add(tm);
        }

        return model;
    }

    private async Task<TeamGameplayModel> InitializeTeamModelAsync(GameplayModel gameplay, UCTeam team)
    {
        var tm = new TeamGameplayModel { TeamId = team.Id, Team = team };

        // Setup team stat
        var tstat = await db.UCGameTeamStats.SingleOrDefaultAsync(
            t => t.TeamId == team.Id && t.GameId == gameplay.GameId);
        if (tstat == null)
        {
            tstat = new UCGameTeamStat { GameId = gameplay.GameId, TeamId = team.Id };
            db.UCGameTeamStats.Add(tstat);
            await db.SaveChangesAsync();
        }
        tm.Stat = tstat;

        // Get all player stats for this game/team
        tm.PlayerStats = await db.UCGamePlayerStats
            .Where(p => p.GameId == gameplay.GameId && p.TeamId == team.Id)
            .ToListAsync();

        // Get quarter player stats
        var game = gameplay.Game;
        var q = game.CurrentQuarter > 4 ? 4 : game.CurrentQuarter;
        tm.QuarterPlayerStats = tm.PlayerStats.Where(p => p.Quarter == q).ToList();

        // Get players and setup stats if needed
        tm.Players = await db.UCPlayers.Where(p => p.TeamId == team.Id).ToListAsync();

        bool hasChanges = false;
        foreach (var player in tm.Players)
        {
            var stat = tm.QuarterPlayerStats.SingleOrDefault(s => s.PlayerId == player.Id);
            if (stat == null)
            {
                stat = new UCGamePlayerStat
                {
                    PlayerId = player.Id,
                    TeamId = player.TeamId,
                    GameId = gameplay.GameId,
                    Quarter = q
                };
                db.UCGamePlayerStats.Add(stat);
                tm.QuarterPlayerStats.Add(stat);
                tm.PlayerStats.Add(stat);
                hasChanges = true;
            }
        }

        if (hasChanges) await db.SaveChangesAsync();

        // Build player models
        var positions = await db.BasketballPositions.ToListAsync();
        foreach (var player in tm.Players)
        {
            var stat = tm.QuarterPlayerStats.FirstOrDefault(s => s.PlayerId == player.Id);
            if (stat != null)
            {
                tm.PlayerModels.Add(new PlayerGameplayModel
                {
                    PlayerId = player.Id,
                    Player = player,
                    Stat = stat,
                    Position = positions.FirstOrDefault(p => p.Id == player.PositionId)
                });
            }
        }

        // Actions
        tm.Actions = await db.UCGameplayActions
            .Where(a => a.GameId == gameplay.GameId && a.TeamId == team.Id)
            .OrderByDescending(a => a.ActionDate)
            .ToListAsync();

        return tm;
    }

    public async Task StartGameAsync(int gameId, IEnumerable<int> start1, IEnumerable<int> start2, int mins, bool autoStart)
    {
        var gp = await CreateAsync(gameId);
        await StartGameInternalAsync(gp, start1, start2, mins, autoStart);
    }

    private async Task StartGameInternalAsync(GameplayModel gp, IEnumerable<int> start1, IEnumerable<int> start2, int mins, bool autoStart)
    {

        void SetStarting5(TeamGameplayModel tm, IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            if (idList.Count == 5)
            {
                foreach (var id in idList)
                {
                    var stat = tm.PlayerStats.FirstOrDefault(s => s.PlayerId == id);
                    if (stat != null) stat.InFloor = true;
                }
            }
        }

        SetStarting5(gp.TeamModels[0], start1);
        SetStarting5(gp.TeamModels[1], start2);

        var game = gp.Game;
        game.CurrentQuarter = 1;
        game.IsStarted = true;
        game.IsTimeOn = autoStart;
        game.TimeLastModified = autoStart ? DateTime.UtcNow : new DateTime(UCConstants.BaseDate.Ticks);
        game.LastActivityDate = DateTime.UtcNow;
        game.TimeLeft = new DateTime(2000, 1, 1, 0, mins, 0, DateTimeKind.Utc);

        await db.SaveChangesAsync();
    }

    public async Task ResetGameAsync(int gameId)
    {
        var actions = await db.UCGameplayActions.Where(a => a.GameId == gameId).ToListAsync();
        db.UCGameplayActions.RemoveRange(actions);

        var pstats = await db.UCGamePlayerStats.Where(p => p.GameId == gameId).ToListAsync();
        db.UCGamePlayerStats.RemoveRange(pstats);

        var tstats = await db.UCGameTeamStats.Where(t => t.GameId == gameId).ToListAsync();
        db.UCGameTeamStats.RemoveRange(tstats);

        var game = await db.UCGames.FindAsync(gameId);
        if (game != null)
        {
            game.IsStarted = false;
            game.IsTimeOn = false;
            game.LastActivityDate = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    public async Task<string> SendActionAsync(int gameId, int teamId, int playerId, int action, int arg, int recPlayerId)
    {
        if (gameId <= 0 || teamId <= 0) return "{\"return\":0}";

        var gp = await CreateAsync(gameId);
        var game = gp.Game;
        var teamModel = gp.TeamModels.Find(t => t.TeamId == teamId);
        var teamModel2 = gp.TeamModels.Find(t => t.TeamId != teamId);
        if (teamModel == null || teamModel2 == null) return "{\"return\":0}";

        var tstat = teamModel.Stat;
        var tstat2 = teamModel2.Stat;
        var now = DateTime.UtcNow;

        tstat.LastUpdate = now;
        if (arg > 0)
        {
            GameHelper.UpdateScore(tstat, game.CurrentQuarter, arg);
        }

        var gameAction = new UCGameplayAction
        {
            GameId = gameId,
            Quarter = game.CurrentQuarter,
            TeamId = teamId,
            PlayerId = playerId,
            ActionCode = action,
            Arg = arg,
            RecPlayerId = recPlayerId,
            ActionDate = now,
            LastUpdate = now,
            GameTime = GameHelper.ComputeTimeRemaining(game),
            TeamScore1 = GameHelper.GetScore(tstat),
            TeamScore2 = GameHelper.GetScore(tstat2),
            Status = 1
        };
        db.UCGameplayActions.Add(gameAction);

        if (playerId > 0)
        {
            var playerModel = teamModel.PlayerModels.Find(p => p.PlayerId == playerId);
            if (playerModel != null)
            {
                PlayerHelper.UpdateStat(playerModel.Stat, (GameActions)action);
                playerModel.Stat.LastUpdate = now;
            }
        }

        game.LastActivityDate = now;
        await db.SaveChangesAsync();

        return "{\"return\":0}";
    }

    public async Task<int> UpdateActionAsync(int id, int mm, int ss)
    {
        if (id <= 0) return 0;

        var action = await db.UCGameplayActions.FindAsync(id);
        if (action == null) return 0;

        var now = DateTime.UtcNow;
        var gp = await CreateAsync(action.GameId);
        var game = gp.Game;
        var teamModel = gp.TeamModels.Find(t => t.TeamId == action.TeamId);
        if (teamModel == null) return 0;

        var tstat = teamModel.Stat;
        tstat.LastUpdate = now;

        if (mm == -1)
        {
            // Toggle Action
            var apply = action.Status == 0;

            if (action.Status == 1) // Active -> UNDO
            {
                action.Status = 0;
                GameHelper.UpdateScore(tstat, action.Quarter, action.Arg * -1);
            }
            else // REDO
            {
                action.Status = 1;
                GameHelper.UpdateScore(tstat, action.Quarter, action.Arg);
            }

            if (action.PlayerId > 0)
            {
                var playerModel = teamModel.PlayerModels.Find(p => p.PlayerId == action.PlayerId);
                if (playerModel != null)
                {
                    PlayerHelper.UpdateStat(playerModel.Stat, (GameActions)action.ActionCode, apply);
                    playerModel.Stat.LastUpdate = now;
                }
            }
        }
        else
        {
            // Update Action Time
            var gt = action.GameTime;
            action.GameTime = new DateTime(gt.Year, gt.Month, gt.Day, gt.Hour, mm, ss, gt.Millisecond);
        }

        action.LastUpdate = now;
        game.LastActivityDate = now;
        await db.SaveChangesAsync();

        return 0;
    }

    public async Task<int> UpdateTimerAsync(int gameId, int start, DateTime timeLeft, int tlMs, DateTime timerLastModified, int tlmMs)
    {
        var gp = await CreateAsync(gameId);
        var game = gp.Game;
        var now = DateTime.UtcNow;

        game.TimeLeft = timeLeft.AddMilliseconds(tlMs).ToUniversalTime();
        game.TimeLastModified = timerLastModified.AddMilliseconds(tlmMs).ToUniversalTime();
        game.IsTimeOn = start == 1;
        game.LastActivityDate = now;
        game.LastUpdate = now;

        await db.SaveChangesAsync();
        return 0;
    }

    public async Task<int> UpdateGameAsync(int gameId, int quarter, bool updateScores, UCGameTeamStat ts0, UCGameTeamStat ts1, bool updateTime, DateTime timeLeft, int tlMs, DateTime timerLastModified, int tlmMs)
    {
        var gp = await CreateAsync(gameId);
        var game = gp.Game;
        var now = DateTime.UtcNow;
        bool hasGameUpdates = false;

        if (updateScores)
        {
            for (int i = 0; i < 2; i++)
            {
                var t = i == 0 ? ts0 : ts1;
                var ts = gp.TeamModels[i].Stat;
                ts.Q1 = t.Q1;
                ts.Q2 = t.Q2;
                ts.Q3 = t.Q3;
                ts.Q4 = t.Q4;
                ts.LastUpdate = DateTime.UtcNow;
            }
        }

        if (updateTime)
        {
            game.TimeLeft = timeLeft.AddMilliseconds(tlMs).ToUniversalTime();
            game.TimeLastModified = timerLastModified.AddMilliseconds(tlmMs).ToUniversalTime();
            hasGameUpdates = true;
        }

        if (game.CurrentQuarter != quarter)
        {
            game.CurrentQuarter = quarter;
            var q = quarter > 4 ? 4 : quarter;

            for (int i = 0; i < 2; i++)
            {
                var teamModel = gp.TeamModels[i];
                var stats = teamModel.QuarterPlayerStats;
                var players = teamModel.Players;

                foreach (var player in players)
                {
                    var stat = stats.SingleOrDefault(s => s.PlayerId == player.Id);
                    if (stat == null)
                    {
                        stat = new UCGamePlayerStat
                        {
                            PlayerId = player.Id,
                            TeamId = player.TeamId,
                            GameId = gp.GameId,
                            Quarter = q
                        };
                        db.UCGamePlayerStats.Add(stat);
                    }
                }
            }
            hasGameUpdates = true;
        }

        if (hasGameUpdates) game.LastUpdate = now;
        if (hasGameUpdates || updateScores)
        {
            game.LastActivityDate = now;
            await db.SaveChangesAsync();
        }

        return 0;
    }

    public async Task<int> IsGameStartedAsync(int gameId)
    {
        var game = await db.UCGames.FindAsync(gameId);
        return game is { IsStarted: true } ? 1 : 0;
    }

    public async Task<string> GetDeltaAsync(int gameId, DateTime lastUpdate)
    {
        var lastUpdateUtc = lastUpdate.ToUniversalTime();
        var gp = await CreateAsync(gameId);
        return BuildDelta(gp, lastUpdateUtc, false);
    }

    public async Task<string> GetDelta2Async(int gameId, DateTime lastUpdate, bool firstSync)
    {
        var lastUpdateUtc = lastUpdate.ToUniversalTime();
        var gp = await CreateAsync(gameId);
        return BuildDelta(gp, lastUpdateUtc, firstSync);
    }

    private static string BuildDelta(GameplayModel model, DateTime lastUpdateUtc, bool firstSync)
    {
        var game = model.Game;

        if (game.LastUpdateForRefresh > lastUpdateUtc)
            return "{\"return\": 2}";

        if (game.LastActivityDate > lastUpdateUtc || firstSync)
        {
            var delta = new StringBuilder();
            delta.Append('{');

            if (game.LastUpdate > lastUpdateUtc || firstSync)
            {
                delta.AppendFormat("\"game\": {0},", JsonSerializer.Serialize(game));
            }

            for (int i = 0; i < 2; i++)
            {
                var teamModel = model.TeamModels[i];
                var stat = teamModel.Stat;

                if (stat.LastUpdate > lastUpdateUtc || firstSync)
                {
                    delta.AppendFormat("\"team{0}\": {{", teamModel.TeamId);
                    delta.AppendFormat("\"ts\": {0}, \"ps\": [", JsonSerializer.Serialize(stat));

                    var sb = new StringBuilder();
                    foreach (var ps in teamModel.PlayerStats)
                    {
                        if (ps.LastUpdate > lastUpdateUtc || firstSync)
                        {
                            sb.AppendFormat("{0},", JsonSerializer.Serialize(ps));
                        }
                    }
                    delta.Append(sb.ToString().TrimEnd(','));
                    delta.Append(']');

                    // team actions (not on first sync)
                    if (!firstSync)
                    {
                        var q = game.CurrentQuarter >= 4 ? 4 : game.CurrentQuarter;
                        var actions = teamModel.Actions
                            .Where(a => (q == 4 ? a.Quarter >= q : a.Quarter == q) &&
                                        (a.ActionDate > lastUpdateUtc || a.LastUpdate > lastUpdateUtc))
                            .OrderByDescending(a => a.ActionDate);

                        delta.Append(",\"actions\": [");
                        sb.Clear();
                        foreach (var action in actions)
                        {
                            sb.AppendFormat("{{\"id\":{0}", action.Id);

                            if (action.ActionDate < action.LastUpdate)
                            {
                                sb.AppendFormat(",\"status\":{0}", action.Status);
                                sb.AppendFormat(",\"time\":\"{0}\"", action.GameTime.ToString("mm:ss"));
                            }
                            else
                            {
                                sb.Append(",\"status\": -1");
                                sb.AppendFormat(",\"action\":{0}", JsonSerializer.Serialize(action));
                                sb.AppendFormat(",\"text\": \"{0}\"",
                                    GameHelper.ActionToString(action, teamModel.Players));
                            }
                            sb.Append("},");
                        }
                        delta.Append(sb.ToString().TrimEnd(','));
                        delta.Append(']');
                    }

                    delta.Append("},");
                }
            }

            delta.AppendFormat("\"lastUpdate\": {0},", JsonSerializer.Serialize(game.LastActivityDate.AddSeconds(1)));
            delta.Append("\"return\": 1}");

            return delta.ToString();
        }

        return "{\"return\": 0}";
    }
}
