using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Web.Pages.Manage;

[Authorize(Roles = "Admin")]
public class GameEntryModel : PageModel
{
    private readonly IGameService _games;
    private readonly ISeasonService _seasons;
    private readonly ITeamService _teams;

    public GameEntryModel(IGameService games, ISeasonService seasons, ITeamService teams)
    {
        _games = games; _seasons = seasons; _teams = teams;
    }

    public int SeasonId { get; set; }
    public bool IsNew => Game == null || Game.Id == 0;
    public UCGame? Game { get; set; }
    public List<UCTeam> TeamList { get; set; } = new();
    public int NextGameNumber { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync(int? id, int? seasonId)
    {
        TeamList = await _teams.GetAllAsync();
        if (id.HasValue)
        {
            Game = await _games.GetByIdAsync(id.Value);
            if (Game == null) return NotFound();
            SeasonId = Game.SeasonId;
        }
        else
        {
            SeasonId = seasonId ?? 0;
            var existing = SeasonId > 0 ? await _games.GetBySeasonAsync(SeasonId) : new();
            NextGameNumber = existing.Count > 0 ? existing.Max(g => g.GameNumber) + 1 : 1;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id, int seasonId, int gameNumber, int team1, int team2, DateTime gameDate, string? venue)
    {
        try
        {
            if (id.HasValue && id > 0)
            {
                await _games.UpdateAsync(id.Value, gameNumber, team1, team2, gameDate, venue ?? "");
            }
            else
            {
                await _games.CreateAsync(seasonId, gameNumber, team1, team2, gameDate, venue ?? "");
            }

            return RedirectToPage("Games", new { seasonId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            TeamList = await _teams.GetAllAsync();
            SeasonId = seasonId;
            if (id.HasValue && id.Value > 0)
            {
                Game = new UCGame
                {
                    Id = id.Value,
                    SeasonId = seasonId,
                    GameNumber = gameNumber,
                    Team1 = team1,
                    Team2 = team2,
                    GameDate = gameDate,
                    Venue = venue ?? ""
                };
            }
            else
            {
                NextGameNumber = gameNumber > 0 ? gameNumber : 1;
            }

            return Page();
        }
    }
}
