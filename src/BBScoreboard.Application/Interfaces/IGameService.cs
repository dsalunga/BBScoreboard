using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Application.Interfaces;

public interface IGameService
{
    Task<List<UCGame>> GetBySeasonAsync(int seasonId);
    Task<UCGame?> GetByIdAsync(int id);
    Task<UCGame> CreateAsync(int seasonId, int gameNumber, int team1, int team2, DateTime gameDate, string venue);
    Task UpdateAsync(UCGame game);
    Task UpdateAsync(int id, int gameNumber, int team1, int team2, DateTime gameDate, string venue);
    Task DeleteAsync(int id);
}
