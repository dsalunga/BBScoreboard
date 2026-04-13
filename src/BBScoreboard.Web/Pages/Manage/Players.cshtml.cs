using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Web.Pages.Manage;

[Authorize(Roles = "Admin")]
public class PlayersModel : PageModel
{
    private readonly IPlayerService _players;
    private readonly ITeamService _teams;
    public PlayersModel(IPlayerService players, ITeamService teams) { _players = players; _teams = teams; }

    public int TeamId { get; set; }
    public UCTeam? Team { get; set; }
    public List<UCPlayer> PlayerList { get; set; } = new();
    public List<BasketballPosition> Positions { get; set; } = new();
    public Dictionary<int, BasketballPosition> PositionMap { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int teamId)
    {
        TeamId = teamId;
        Team = await _teams.GetByIdAsync(teamId);
        if (Team == null) return NotFound();
        PlayerList = await _players.GetByTeamAsync(teamId);
        Positions = await _players.GetPositionsAsync();
        PositionMap = Positions.ToDictionary(p => p.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(int teamId, string firstName, string lastName, int playerNumber, int positionId)
    {
        await _players.CreateAsync(firstName.Trim(), lastName.Trim(), playerNumber, positionId, teamId, true);
        return RedirectToPage(new { teamId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, int teamId)
    {
        await _players.DeleteAsync(id);
        return RedirectToPage(new { teamId });
    }
}
