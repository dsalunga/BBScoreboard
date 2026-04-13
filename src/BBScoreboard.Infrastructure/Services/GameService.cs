using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;
using BBScoreboard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BBScoreboard.Infrastructure.Services;

public class GameService(BBScoreboardDbContext db) : IGameService
{
    public async Task<List<UCGame>> GetBySeasonAsync(int seasonId)
    {
        return await db.UCGames.Where(g => g.SeasonId == seasonId).OrderBy(g => g.GameNumber).ToListAsync();
    }

    public async Task<UCGame?> GetByIdAsync(int id)
    {
        return await db.UCGames.FindAsync(id);
    }

    public async Task<UCGame> CreateAsync(int seasonId, int gameNumber, int team1, int team2, DateTime gameDate, string venue)
    {
        var now = DateTime.UtcNow;
        var game = new UCGame
        {
            SeasonId = seasonId,
            GameNumber = gameNumber,
            Team1 = team1,
            Team2 = team2,
            GameDate = gameDate,
            Venue = venue,
            TimeLastModified = now,
            TimeLeft = now,
            LastActivityDate = now,
            LastUpdate = now,
            LastUpdateForRefresh = now
        };
        db.UCGames.Add(game);
        await db.SaveChangesAsync();
        return game;
    }

    public async Task UpdateAsync(UCGame game)
    {
        db.UCGames.Update(game);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, int gameNumber, int team1, int team2, DateTime gameDate, string venue)
    {
        var game = await db.UCGames.FindAsync(id);
        if (game != null)
        {
            game.GameNumber = gameNumber;
            game.Team1 = team1;
            game.Team2 = team2;
            game.GameDate = gameDate;
            game.Venue = venue;
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        if (id <= 0) return;

        var actions = await db.UCGameplayActions.Where(a => a.GameId == id).ToListAsync();
        db.UCGameplayActions.RemoveRange(actions);

        var pstats = await db.UCGamePlayerStats.Where(p => p.GameId == id).ToListAsync();
        db.UCGamePlayerStats.RemoveRange(pstats);

        var tstats = await db.UCGameTeamStats.Where(t => t.GameId == id).ToListAsync();
        db.UCGameTeamStats.RemoveRange(tstats);

        var game = await db.UCGames.FindAsync(id);
        if (game != null)
        {
            db.UCGames.Remove(game);
            await db.SaveChangesAsync();
        }
    }
}
