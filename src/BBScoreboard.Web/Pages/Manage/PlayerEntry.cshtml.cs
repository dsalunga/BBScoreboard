using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Web.Pages.Manage;

[Authorize(Roles = "Admin")]
public class PlayerEntryModel : PageModel
{
    private readonly IPlayerService _players;
    public PlayerEntryModel(IPlayerService players) => _players = players;

    public UCPlayer? Player { get; set; }
    public List<BasketballPosition> Positions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Player = await _players.GetByIdAsync(id);
        if (Player == null) return NotFound();
        Positions = await _players.GetPositionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, string firstName, string lastName, int playerNumber, int positionId, bool active, int teamId)
    {
        await _players.UpdateAsync(id, firstName.Trim(), lastName.Trim(), playerNumber, positionId, active);
        return RedirectToPage("Players", new { teamId });
    }
}
