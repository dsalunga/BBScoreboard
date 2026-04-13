using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Application;
using BBScoreboard.Application.Interfaces;
using BBScoreboard.Application.Services;
using BBScoreboard.Domain;
using BBScoreboard.Domain.Entities;
using BBScoreboard.Domain.Enums;

namespace BBScoreboard.Web.Pages.Gameplay;

[Authorize(Roles = "Admin,Scorer")]
public class ManagerModel : PageModel
{
    private readonly IGameplayService _gameplay;
    private readonly IGameService _games;
    private readonly ITeamService _teams;
    private readonly IAppConfigService _config;

    public ManagerModel(IGameplayService gameplay, IGameService games, ITeamService teams, IAppConfigService config)
    {
        _gameplay = gameplay;
        _games = games;
        _teams = teams;
        _config = config;
    }

    public GameplayModel? Gp { get; set; }
    public Dictionary<int, UCTeam> TeamMap { get; set; } = new();
    public bool EnableTimer { get; set; }
    public bool ShowAllActions { get; set; }
    public bool IsAdmin { get; set; }
    [TempData]
    public string? ManagerError { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id, int? game)
    {
        var gameId = id ?? game;
        if (!gameId.HasValue || gameId.Value <= 0) return NotFound();

        EnableTimer = await _config.GetBoolAsync("EnableTimer", true);
        ShowAllActions = await _config.GetBoolAsync("ShowAllActions", false);
        IsAdmin = User.IsInRole("Admin");
        Gp = await _gameplay.BuildGameplayModelAsync(gameId.Value);
        if (Gp == null) return NotFound();
        var allTeams = await _teams.GetAllAsync();
        TeamMap = allTeams.ToDictionary(t => t.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int gameId, string cmd)
    {
        if (gameId <= 0)
        {
            return BadRequest();
        }

        try
        {
            switch (cmd)
            {
                case "Start":
                    await HandleStartAsync(gameId);
                    break;
                case "Reset":
                case "Lock":
                case "UpdateGame":
                    if (!User.IsInRole("Admin"))
                    {
                        return Forbid();
                    }

                    if (cmd == "Reset")
                    {
                        await _gameplay.ResetGameAsync(gameId);
                    }
                    else if (cmd == "Lock")
                    {
                        var game = await _games.GetByIdAsync(gameId);
                        if (game != null)
                        {
                            game.IsEnded = !game.IsEnded;
                            await _games.UpdateAsync(game);
                        }
                    }
                    else
                    {
                        await HandleUpdateGameAsync(gameId);
                    }
                    break;
                default:
                    ManagerError = "Unknown command.";
                    break;
            }
        }
        catch (InvalidOperationException ex)
        {
            ManagerError = ex.Message;
        }
        catch (FormatException)
        {
            ManagerError = "Invalid numeric input.";
        }
        catch (OverflowException)
        {
            ManagerError = "One or more values are out of range.";
        }
        return RedirectToPage(new { id = gameId });
    }

    private async Task HandleStartAsync(int gameId)
    {
        var players0 = ParsePlayerIds("players0[]");
        var players1 = ParsePlayerIds("players1[]");
        var starters = players0.Concat(players1).ToList();
        var smm = ParseRequiredInt("smm", 20);
        if (smm is < 1 or > 59)
        {
            throw new InvalidOperationException("Game time must be between 1 and 59 minutes.");
        }

        var autoTimer = Request.Form["autoTimer"].FirstOrDefault() == "true";
        await _gameplay.StartGameAsync(gameId, starters, smm, autoTimer);
    }

    private async Task HandleUpdateGameAsync(int gameId)
    {
        var game = await _games.GetByIdAsync(gameId);
        if (game == null) return;
        game.CurrentQuarter = ParseRequiredInt("quarter", 1);
        if (game.CurrentQuarter < 1)
        {
            throw new InvalidOperationException("Quarter must be 1 or greater.");
        }

        var updateScores = Request.Form["updateScores"].FirstOrDefault() == "true";
        await _games.UpdateAsync(game);

        if (updateScores)
        {
            var gp = await _gameplay.BuildGameplayModelAsync(gameId);
            if (gp != null)
            {
                for (int t = 0; t < 2; t++)
                {
                    var stat = gp.TeamModels[t].Stat;
                    stat.Q1 = ParseRequiredInt($"ts{t}_q1", 0);
                    stat.Q2 = ParseRequiredInt($"ts{t}_q2", 0);
                    stat.Q3 = ParseRequiredInt($"ts{t}_q3", 0);
                    stat.Q4 = ParseRequiredInt($"ts{t}_q4", 0);
                    await _gameplay.UpdateTeamStatAsync(stat);
                }
            }
        }
    }

    private List<int> ParsePlayerIds(string key)
    {
        var ids = new List<int>();
        foreach (var value in Request.Form[key])
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (!int.TryParse(value, out var id))
            {
                throw new InvalidOperationException("Invalid player selection.");
            }

            ids.Add(id);
        }

        return ids;
    }

    private int ParseRequiredInt(string key, int fallback)
    {
        var value = Request.Form[key].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        if (!int.TryParse(value, out var parsed))
        {
            throw new InvalidOperationException($"Invalid value for '{key}'.");
        }

        return parsed;
    }

    public string GetTeamName(int teamId) => TeamMap.TryGetValue(teamId, out var t) ? t.Name : $"#{teamId}";
}
