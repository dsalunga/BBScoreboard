using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;
using BBScoreboard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BBScoreboard.Infrastructure.Services;

public class AppConfigService(BBScoreboardDbContext db) : IAppConfigService
{
    public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
    {
        var config = await db.AppConfigs.FirstOrDefaultAsync(i => i.Key == key);
        if (config == null) return defaultValue;
        return config.Value == "1" || config.Value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> GetShowAllActionsAsync()
    {
        var config = await db.AppConfigs.FirstOrDefaultAsync(i => i.Key == "ShowAllActions");
        return config != null && (config.Value == "1" || config.Value.Equals("true", StringComparison.OrdinalIgnoreCase));
    }

    public async Task SetShowAllActionsAsync(bool value)
    {
        await SetValueAsync("ShowAllActions", value ? "1" : "0");
    }

    public async Task<bool> GetEnableTimerAsync()
    {
        var config = await db.AppConfigs.FirstOrDefaultAsync(i => i.Key == "EnableTimer");
        return config != null && (config.Value == "1" || config.Value.Equals("true", StringComparison.OrdinalIgnoreCase));
    }

    public async Task SetEnableTimerAsync(bool value)
    {
        await SetValueAsync("EnableTimer", value ? "1" : "0");
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var config = await db.AppConfigs.FirstOrDefaultAsync(i => i.Key == key);
        return config?.Value;
    }

    public async Task SetValueAsync(string key, string value)
    {
        var config = await db.AppConfigs.FirstOrDefaultAsync(i => i.Key == key);
        if (config == null)
        {
            config = new AppConfig { Key = key, Value = value };
            db.AppConfigs.Add(config);
        }
        else
        {
            config.Value = value;
        }
        await db.SaveChangesAsync();
    }
}
