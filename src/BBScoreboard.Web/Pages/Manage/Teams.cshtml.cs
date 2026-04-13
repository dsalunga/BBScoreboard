using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Web.Pages.Manage;

[Authorize(Roles = "Admin")]
public class TeamsModel : PageModel
{
    private readonly ITeamService _teams;
    public TeamsModel(ITeamService teams) => _teams = teams;

    public List<UCTeam> TeamList { get; set; } = new();

    public async Task OnGetAsync()
    {
        TeamList = await _teams.GetAllAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync(string name, string teamColor)
    {
        if (!string.IsNullOrWhiteSpace(name))
            await _teams.CreateAsync(name.Trim(), teamColor ?? "#000000", true);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _teams.DeleteAsync(id);
        return RedirectToPage();
    }
}
