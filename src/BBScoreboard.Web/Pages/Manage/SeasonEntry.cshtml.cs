using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Web.Pages.Manage;

[Authorize(Roles = "Admin")]
public class SeasonEntryModel : PageModel
{
    private readonly ISeasonService _seasons;
    public SeasonEntryModel(ISeasonService seasons) => _seasons = seasons;

    public UCSeason? Season { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Season = await _seasons.GetByIdAsync(id);
        return Season == null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, string name)
    {
        await _seasons.UpdateAsync(id, name.Trim());
        return RedirectToPage("Seasons");
    }
}
