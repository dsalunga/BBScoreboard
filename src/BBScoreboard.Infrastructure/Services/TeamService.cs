using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;
using BBScoreboard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BBScoreboard.Infrastructure.Services;

public class TeamService(BBScoreboardDbContext db) : ITeamService
{
    public async Task<List<UCTeam>> GetAllAsync()
    {
        return await db.UCTeams.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<UCTeam?> GetByIdAsync(int id)
    {
        return await db.UCTeams.FindAsync(id);
    }

    public async Task<UCTeam> CreateAsync(string name, string teamColor, bool active)
    {
        var team = new UCTeam { Name = name, TeamColor = teamColor, Active = active };
        db.UCTeams.Add(team);
        await db.SaveChangesAsync();
        return team;
    }

    public async Task UpdateAsync(int id, string name, string teamColor, bool active)
    {
        var team = await db.UCTeams.FindAsync(id);
        if (team != null)
        {
            team.Name = name;
            team.TeamColor = teamColor;
            team.Active = active;
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        if (id <= 0) return;

        // Cascade: delete team stats
        var tstats = await db.UCGameTeamStats.Where(t => t.TeamId == id).ToListAsync();
        db.UCGameTeamStats.RemoveRange(tstats);

        // Cascade: delete all players in this team
        var players = await db.UCPlayers.Where(p => p.TeamId == id).ToListAsync();
        foreach (var player in players)
        {
            await DeletePlayerCascadeAsync(player.Id);
        }

        var team = await db.UCTeams.FindAsync(id);
        if (team != null)
        {
            db.UCTeams.Remove(team);
            await db.SaveChangesAsync();
        }
    }

    private async Task DeletePlayerCascadeAsync(int playerId)
    {
        var actions = await db.UCGameplayActions.Where(a => a.PlayerId == playerId).ToListAsync();
        db.UCGameplayActions.RemoveRange(actions);

        var pstats = await db.UCGamePlayerStats.Where(p => p.PlayerId == playerId).ToListAsync();
        db.UCGamePlayerStats.RemoveRange(pstats);

        var player = await db.UCPlayers.FindAsync(playerId);
        if (player != null) db.UCPlayers.Remove(player);
    }
}
