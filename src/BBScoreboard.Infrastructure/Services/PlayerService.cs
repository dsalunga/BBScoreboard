using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;
using BBScoreboard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BBScoreboard.Infrastructure.Services;

public class PlayerService(BBScoreboardDbContext db) : IPlayerService
{
    public async Task<List<UCPlayer>> GetAllAsync()
    {
        return await db.UCPlayers.OrderBy(p => p.PlayerNumber).ToListAsync();
    }

    public async Task<List<BasketballPosition>> GetPositionsAsync()
    {
        return await db.BasketballPositions.OrderBy(p => p.Id).ToListAsync();
    }

    public async Task<List<UCPlayer>> GetByTeamAsync(int teamId)
    {
        return await db.UCPlayers.Where(p => p.TeamId == teamId).OrderBy(p => p.PlayerNumber).ToListAsync();
    }

    public async Task<UCPlayer?> GetByIdAsync(int id)
    {
        return await db.UCPlayers.FindAsync(id);
    }

    public async Task<UCPlayer> CreateAsync(string firstName, string lastName, int playerNumber, int positionId, int teamId, bool active)
    {
        var player = new UCPlayer
        {
            FirstName = firstName,
            LastName = lastName,
            PlayerNumber = playerNumber,
            PositionId = positionId,
            TeamId = teamId,
            Active = active
        };
        db.UCPlayers.Add(player);
        await db.SaveChangesAsync();
        return player;
    }

    public async Task UpdateAsync(int id, string firstName, string lastName, int playerNumber, int positionId, bool active)
    {
        var player = await db.UCPlayers.FindAsync(id);
        if (player != null)
        {
            player.FirstName = firstName;
            player.LastName = lastName;
            player.PlayerNumber = playerNumber;
            player.PositionId = positionId;
            player.Active = active;
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        if (id <= 0) return;

        var actions = await db.UCGameplayActions.Where(a => a.PlayerId == id).ToListAsync();
        db.UCGameplayActions.RemoveRange(actions);

        var pstats = await db.UCGamePlayerStats.Where(p => p.PlayerId == id).ToListAsync();
        db.UCGamePlayerStats.RemoveRange(pstats);

        var player = await db.UCPlayers.FindAsync(id);
        if (player != null)
        {
            db.UCPlayers.Remove(player);
            await db.SaveChangesAsync();
        }
    }
}
