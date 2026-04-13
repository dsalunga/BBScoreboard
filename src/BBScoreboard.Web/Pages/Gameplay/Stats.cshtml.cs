using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Application;
using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Web.Pages.Gameplay;

[Authorize]
public class StatsModel : PageModel
{
    private readonly IGameplayService _gameplay;
    private readonly ITeamService _teams;
    private readonly IPlayerService _players;

    public StatsModel(IGameplayService gameplay, ITeamService teams, IPlayerService players)
    {
        _gameplay = gameplay;
        _teams = teams;
        _players = players;
    }

    public GameplayModel? Gp { get; set; }
    public Dictionary<int, UCTeam> TeamMap { get; set; } = new();
    public Dictionary<int, BasketballPosition> Positions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id, int? game)
    {
        var gameId = id ?? game;
        if (!gameId.HasValue || gameId.Value <= 0) return NotFound();

        Gp = await _gameplay.BuildGameplayModelAsync(gameId.Value);
        if (Gp == null) return NotFound();
        var allTeams = await _teams.GetAllAsync();
        TeamMap = allTeams.ToDictionary(t => t.Id);
        Positions = (await _players.GetPositionsAsync()).ToDictionary(p => p.Id);
        return Page();
    }

    public string GetTeamName(int teamId) => TeamMap.TryGetValue(teamId, out var t) ? t.Name : $"#{teamId}";
}
