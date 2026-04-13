using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;
using BBScoreboard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BBScoreboard.Infrastructure.Services;

public class SeasonService(BBScoreboardDbContext db) : ISeasonService
{
    public async Task<List<UCSeason>> GetAllAsync()
    {
        return await db.UCSeasons.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<UCSeason?> GetByIdAsync(int id)
    {
        return await db.UCSeasons.FindAsync(id);
    }

    public async Task<UCSeason> CreateAsync(string name)
    {
        var season = new UCSeason { Name = name };
        db.UCSeasons.Add(season);
        await db.SaveChangesAsync();
        return season;
    }

    public async Task UpdateAsync(int id, string name)
    {
        var season = await db.UCSeasons.FindAsync(id);
        if (season != null)
        {
            season.Name = name;
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        if (id <= 0) return;

        // Cascade: delete all games in this season
        var games = await db.UCGames.Where(g => g.SeasonId == id).ToListAsync();
        foreach (var game in games)
        {
            await DeleteGameCascadeAsync(game.Id);
        }

        var season = await db.UCSeasons.FindAsync(id);
        if (season != null)
        {
            db.UCSeasons.Remove(season);
            await db.SaveChangesAsync();
        }
    }

    private async Task DeleteGameCascadeAsync(int gameId)
    {
        var actions = await db.UCGameplayActions.Where(a => a.GameId == gameId).ToListAsync();
        db.UCGameplayActions.RemoveRange(actions);

        var pstats = await db.UCGamePlayerStats.Where(p => p.GameId == gameId).ToListAsync();
        db.UCGamePlayerStats.RemoveRange(pstats);

        var tstats = await db.UCGameTeamStats.Where(t => t.GameId == gameId).ToListAsync();
        db.UCGameTeamStats.RemoveRange(tstats);

        var game = await db.UCGames.FindAsync(gameId);
        if (game != null) db.UCGames.Remove(game);
    }
}
