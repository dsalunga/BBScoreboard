using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BBScoreboard.Web.Pages.Gameplay;

[Authorize]
public class GamesModel(ISeasonService seasonService, IGameService gameService, ITeamService teamService) : PageModel
{
    public List<UCSeason> Seasons { get; set; } = [];
    public List<UCGame> Games { get; set; } = [];
    public List<UCTeam> Teams { get; set; } = [];
    public int SeasonId { get; set; }
    public string View { get; set; } = "Manager";

    public async Task OnGetAsync(int? season, string? view)
    {
        View = view ?? "Manager";
        Seasons = await seasonService.GetAllAsync();
        Teams = await teamService.GetAllAsync();

        SeasonId = season ?? Seasons.FirstOrDefault()?.Id ?? 0;
        if (SeasonId > 0)
        {
            Games = await gameService.GetBySeasonAsync(SeasonId);
        }
    }

    public string GetTeamName(int teamId)
    {
        return Teams.FirstOrDefault(t => t.Id == teamId)?.Name ?? "";
    }
}
