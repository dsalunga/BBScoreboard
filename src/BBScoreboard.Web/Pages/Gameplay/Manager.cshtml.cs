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

[Authorize]
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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        EnableTimer = await _config.GetBoolAsync("EnableTimer", true);
        ShowAllActions = await _config.GetBoolAsync("ShowAllActions", false);
        IsAdmin = User.IsInRole("Admin");
        Gp = await _gameplay.BuildGameplayModelAsync(id);
        if (Gp == null) return NotFound();
        var allTeams = await _teams.GetAllAsync();
        TeamMap = allTeams.ToDictionary(t => t.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int gameId, string cmd)
    {
        switch (cmd)
        {
            case "Start":
                await HandleStartAsync(gameId);
                break;
            case "Reset":
                await _gameplay.ResetGameAsync(gameId);
                break;
            case "Lock":
                var game = await _games.GetByIdAsync(gameId);
                if (game != null)
                {
                    game.IsEnded = !game.IsEnded;
                    await _games.UpdateAsync(game);
                }
                break;
            case "UpdateGame":
                await HandleUpdateGameAsync(gameId);
                break;
        }
        return RedirectToPage(new { id = gameId });
    }

    private async Task HandleStartAsync(int gameId)
    {
        var players0 = Request.Form["players0[]"].Where(s => s != null).Select(s => int.Parse(s!)).ToList();
        var players1 = Request.Form["players1[]"].Where(s => s != null).Select(s => int.Parse(s!)).ToList();
        var starters = players0.Concat(players1).ToList();
        var smm = int.Parse(Request.Form["smm"].FirstOrDefault() ?? "20");
        var autoTimer = Request.Form["autoTimer"].FirstOrDefault() == "true";
        await _gameplay.StartGameAsync(gameId, starters, smm, autoTimer);
    }

    private async Task HandleUpdateGameAsync(int gameId)
    {
        var game = await _games.GetByIdAsync(gameId);
        if (game == null) return;
        game.CurrentQuarter = int.Parse(Request.Form["quarter"].FirstOrDefault() ?? "1");
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
                    stat.Q1 = int.Parse(Request.Form[$"ts{t}_q1"].FirstOrDefault() ?? "0");
                    stat.Q2 = int.Parse(Request.Form[$"ts{t}_q2"].FirstOrDefault() ?? "0");
                    stat.Q3 = int.Parse(Request.Form[$"ts{t}_q3"].FirstOrDefault() ?? "0");
                    stat.Q4 = int.Parse(Request.Form[$"ts{t}_q4"].FirstOrDefault() ?? "0");
                    await _gameplay.UpdateTeamStatAsync(stat);
                }
            }
        }
    }

    public string GetTeamName(int teamId) => TeamMap.TryGetValue(teamId, out var t) ? t.Name : $"#{teamId}";
}
