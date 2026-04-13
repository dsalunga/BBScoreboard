using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Application.Interfaces;

public interface ITeamService
{
    Task<List<UCTeam>> GetAllAsync();
    Task<UCTeam?> GetByIdAsync(int id);
    Task<UCTeam> CreateAsync(string name, string teamColor, bool active);
    Task UpdateAsync(int id, string name, string teamColor, bool active);
    Task DeleteAsync(int id);
}
