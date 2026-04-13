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
        await ValidateGameAsync(seasonId, gameNumber, team1, team2, gameDate, null);

        var now = DateTime.UtcNow;
        var game = new UCGame
        {
            SeasonId = seasonId,
            GameNumber = gameNumber,
            Team1 = team1,
            Team2 = team2,
            GameDate = NormalizeGameDateUtc(gameDate),
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
        if (game == null) throw new InvalidOperationException("Game not found.");

        await ValidateGameAsync(game.SeasonId, gameNumber, team1, team2, gameDate, id);

        game.GameNumber = gameNumber;
        game.Team1 = team1;
        game.Team2 = team2;
        game.GameDate = NormalizeGameDateUtc(gameDate);
        game.Venue = venue;
        await db.SaveChangesAsync();
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

    private async Task ValidateGameAsync(int seasonId, int gameNumber, int team1, int team2, DateTime gameDate, int? existingGameId)
    {
        if (seasonId <= 0) throw new InvalidOperationException("A valid season is required.");
        if (gameNumber <= 0) throw new InvalidOperationException("Game number must be greater than zero.");
        if (team1 <= 0 || team2 <= 0) throw new InvalidOperationException("Both teams are required.");
        if (team1 == team2) throw new InvalidOperationException("Team 1 and Team 2 must be different.");
        if (gameDate == default) throw new InvalidOperationException("Game date is required.");

        if (!await db.UCSeasons.AnyAsync(s => s.Id == seasonId))
        {
            throw new InvalidOperationException("Selected season does not exist.");
        }

        var teamCount = await db.UCTeams.CountAsync(t => t.Id == team1 || t.Id == team2);
        if (teamCount != 2)
        {
            throw new InvalidOperationException("One or more selected teams do not exist.");
        }

        var duplicateExists = await db.UCGames.AnyAsync(g =>
            g.SeasonId == seasonId &&
            g.GameNumber == gameNumber &&
            (!existingGameId.HasValue || g.Id != existingGameId.Value));
        if (duplicateExists)
        {
            throw new InvalidOperationException("Game number already exists in this season.");
        }
    }

    private static DateTime NormalizeGameDateUtc(DateTime gameDate)
    {
        var dateOnly = gameDate.Date;
        return DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc);
    }
}
