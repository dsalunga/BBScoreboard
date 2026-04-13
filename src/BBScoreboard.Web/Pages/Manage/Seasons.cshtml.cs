using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Web.Pages.Manage;

[Authorize(Roles = "Admin")]
public class SeasonsModel : PageModel
{
    private readonly ISeasonService _seasons;
    public SeasonsModel(ISeasonService seasons) => _seasons = seasons;

    public List<UCSeason> SeasonList { get; set; } = new();

    public async Task OnGetAsync()
    {
        SeasonList = await _seasons.GetAllAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
            await _seasons.CreateAsync(name.Trim());
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _seasons.DeleteAsync(id);
        return RedirectToPage();
    }
}
