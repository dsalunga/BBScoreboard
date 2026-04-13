namespace BBScoreboard.Application.Interfaces;

public interface IAppConfigService
{
    Task<bool> GetBoolAsync(string key, bool defaultValue = false);
    Task<bool> GetShowAllActionsAsync();
    Task SetShowAllActionsAsync(bool value);
    Task<bool> GetEnableTimerAsync();
    Task SetEnableTimerAsync(bool value);
    Task<string?> GetValueAsync(string key);
    Task SetValueAsync(string key, string value);
}
