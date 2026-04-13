using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Web.Pages.Manage;

[Authorize(Roles = "Admin")]
public class GamesModel : PageModel
{
    private readonly IGameService _games;
    private readonly ISeasonService _seasons;
    private readonly ITeamService _teams;

    public GamesModel(IGameService games, ISeasonService seasons, ITeamService teams)
    {
        _games = games; _seasons = seasons; _teams = teams;
    }

    public int SeasonId { get; set; }
    public List<UCSeason> Seasons { get; set; } = new();
    public List<UCGame> GameList { get; set; } = new();
    public Dictionary<int, UCTeam> TeamMap { get; set; } = new();

    public async Task OnGetAsync(int? seasonId)
    {
        Seasons = await _seasons.GetAllAsync();
        SeasonId = seasonId ?? Seasons.FirstOrDefault()?.Id ?? 0;
        GameList = SeasonId > 0 ? await _games.GetBySeasonAsync(SeasonId) : new();
        TeamMap = (await _teams.GetAllAsync()).ToDictionary(t => t.Id);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, int seasonId)
    {
        await _games.DeleteAsync(id);
        return RedirectToPage(new { seasonId });
    }

    public string GetTeamName(int teamId) => TeamMap.TryGetValue(teamId, out var t) ? t.Name : $"#{teamId}";
}
