using BBScoreboard.Domain;
using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Application.Services;

public static class GameHelper
{
    public static DateTime ComputeTimeRemaining(UCGame game)
    {
        if (game.IsTimeOn)
        {
            var timeLeft = game.TimeLeft - (DateTime.UtcNow - game.TimeLastModified);
            return UCConstants.BaseDate > timeLeft ? UCConstants.BaseDate : timeLeft;
        }
        return game.TimeLeft;
    }

    public static void UpdateScore(UCGameTeamStat stat, int quarter, int scoreDelta)
    {
        if (scoreDelta == 0) return;

        switch (quarter)
        {
            case 1: stat.Q1 += scoreDelta; break;
            case 2: stat.Q2 += scoreDelta; break;
            case 3: stat.Q3 += scoreDelta; break;
            default: stat.Q4 += scoreDelta; break;
        }
    }

    public static int GetScore(UCGameTeamStat stat)
    {
        return stat.Q1 + stat.Q2 + stat.Q3 + stat.Q4;
    }

    public static string GetTeamName(IEnumerable<UCTeam> teams, int id)
    {
        var team = id > 0 ? teams.SingleOrDefault(i => i.Id == id) : null;
        return team?.Name ?? "";
    }

    public static string GetTeamName(IEnumerable<UCTeam> teams, int team1, int team2, string sep = "vs")
    {
        return $"{GetTeamName(teams, team1)} {sep} {GetTeamName(teams, team2)}";
    }

    public static string GetQuarterString(int currentQuarter)
    {
        return currentQuarter > 4 ? $"OT{currentQuarter - 4}" : $"Q{currentQuarter}";
    }

    public static string ActionToString(UCGameplayAction action, IEnumerable<UCPlayer> players)
    {
        var player = action.PlayerId > 0 ? players.SingleOrDefault(i => i.Id == action.PlayerId) : null;

        return string.Format("Q{5} <strong>{0}</strong> {1} ({2})... {3}-{4}",
            action.GameTime.ToString("mm:ss"),
            GameAction.GetAction(action.ActionCode).Text,
            player == null ? "" : $"{player.PlayerNumber} {player.FirstName[..1]}. {player.LastName}",
            action.TeamScore1,
            action.TeamScore2,
            action.Quarter);
    }
}
