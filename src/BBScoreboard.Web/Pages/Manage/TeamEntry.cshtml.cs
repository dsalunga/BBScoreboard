using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Web.Pages.Manage;

[Authorize(Roles = "Admin")]
public class TeamEntryModel : PageModel
{
    private readonly ITeamService _teams;
    public TeamEntryModel(ITeamService teams) => _teams = teams;

    public UCTeam? Team { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Team = await _teams.GetByIdAsync(id);
        return Team == null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, string name, string teamColor, bool active)
    {
        await _teams.UpdateAsync(id, name.Trim(), teamColor ?? "#000000", active);
        return RedirectToPage("Teams");
    }
}
