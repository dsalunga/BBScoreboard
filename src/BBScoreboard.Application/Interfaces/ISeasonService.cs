using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Application.Interfaces;

public interface ISeasonService
{
    Task<List<UCSeason>> GetAllAsync();
    Task<UCSeason?> GetByIdAsync(int id);
    Task<UCSeason> CreateAsync(string name);
    Task UpdateAsync(int id, string name);
    Task DeleteAsync(int id);
}
