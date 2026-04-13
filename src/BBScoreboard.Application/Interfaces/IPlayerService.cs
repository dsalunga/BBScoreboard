using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Application.Interfaces;

public interface IPlayerService

{
    Task<List<UCPlayer>> GetByTeamAsync(int teamId);
    Task<List<UCPlayer>> GetAllAsync();
    Task<List<BasketballPosition>> GetPositionsAsync();
    Task<UCPlayer?> GetByIdAsync(int id);
    Task<UCPlayer> CreateAsync(string firstName, string lastName, int playerNumber, int positionId, int teamId, bool active);
    Task UpdateAsync(int id, string firstName, string lastName, int playerNumber, int positionId, bool active);
    Task DeleteAsync(int id);
}
