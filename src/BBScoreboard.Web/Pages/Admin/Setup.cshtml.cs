using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class SetupModel : PageModel
{
    private readonly IAppConfigService _config;
    private readonly ISeasonService _seasons;
    private readonly ITeamService _teams;
    private readonly IPlayerService _players;
    private readonly IGameService _games;

    public SetupModel(
        IAppConfigService config,
        ISeasonService seasons,
        ITeamService teams,
        IPlayerService players,
        IGameService games)
    {
        _config = config;
        _seasons = seasons;
        _teams = teams;
        _players = players;
        _games = games;
    }

    public bool EnableTimer { get; set; }
    public bool ShowAllActions { get; set; }
    public string? Message { get; set; }
    public int? DemoGameId { get; set; }

    public async Task OnGetAsync()
    {
        await LoadSettingsAsync();
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

    public async Task<IActionResult> OnPostCreateDemoAsync()
    {
        await LoadSettingsAsync();

        try
        {
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var season = await _seasons.CreateAsync($"Demo {stamp}");

            var home = await _teams.CreateAsync($"Demo Home {stamp}", "#1f77b4", true);
            var away = await _teams.CreateAsync($"Demo Away {stamp}", "#d62728", true);

            var positions = await _players.GetPositionsAsync();
            if (positions.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No basketball positions available. Please seed positions first.");
                return Page();
            }

            await CreateDemoRosterAsync(home.Id, "Home", positions);
            await CreateDemoRosterAsync(away.Id, "Away", positions);

            var game = await _games.CreateAsync(
                season.Id,
                gameNumber: 1,
                team1: home.Id,
                team2: away.Id,
                gameDate: DateTime.UtcNow,
                venue: "Demo Arena");

            Message = "Demo setup created. You can now open Manager and start/play the game.";
            DemoGameId = game.Id;
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return Page();
    }

    private async Task LoadSettingsAsync()
    {
        EnableTimer = await _config.GetEnableTimerAsync();
        ShowAllActions = await _config.GetShowAllActionsAsync();
    }

    private async Task CreateDemoRosterAsync(int teamId, string label, List<BasketballPosition> positions)
    {
        for (var i = 0; i < 8; i++)
        {
            var position = positions[i % positions.Count];
            await _players.CreateAsync(
                firstName: label,
                lastName: $"Player{i + 1}",
                playerNumber: i + 1,
                positionId: position.Id,
                teamId: teamId,
                active: true);
        }
    }
}
