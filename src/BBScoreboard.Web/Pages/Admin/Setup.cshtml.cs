using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Application.Interfaces;

namespace BBScoreboard.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class SetupModel : PageModel
{
    private readonly IAppConfigService _config;
    public SetupModel(IAppConfigService config) => _config = config;

    public bool EnableTimer { get; set; }
    public bool ShowAllActions { get; set; }
    public string? Message { get; set; }

    public async Task OnGetAsync()
    {
        EnableTimer = await _config.GetEnableTimerAsync();
        ShowAllActions = await _config.GetShowAllActionsAsync();
    }

    public async Task<IActionResult> OnPostAsync(bool enableTimer, bool showAllActions)
    {
        await _config.SetEnableTimerAsync(enableTimer);
        await _config.SetShowAllActionsAsync(showAllActions);
        Message = "Settings saved.";
        EnableTimer = enableTimer;
        ShowAllActions = showAllActions;
        return Page();
    }
}
