using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBScoreboard
{
    public class GameHelper
    {
        public static string GetTeamName(IEnumerable<UCTeam> teams, int id)
        {
            UCTeam team = null;
            return (team = id > 0 ? teams.Single(i => i.Id == id) : null) == null ? "" : team.Name;
        }

        public static string GetTeamName(IEnumerable<UCTeam> teams, int team1, int team2, string sep = "vs")
        {
            return string.Format("{0} {1} {2}", GetTeamName(teams, team1), sep, GetTeamName(teams, team2));
        }

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
            if (scoreDelta == 0)
                return;

            switch (quarter)
            {
                case 1:
                    stat.Q1 += scoreDelta;
                    break;
                case 2:
                    stat.Q2 += scoreDelta;
                    break;
                case 3:
                    stat.Q3 += scoreDelta;
                    break;
                default:
                    stat.Q4 += scoreDelta;
                    break;
            }
        }

        public static int GetScore(UCGameTeamStat stat)
        {
            return stat.Q1 + stat.Q2 + stat.Q3 + stat.Q4;
        }

        public static void Delete(int id, BBScoreboardEntities db = null)
        {
            if (id > 0)
            {
                db = db ?? new BBScoreboardEntities();

                var actions = db.UCGameplayActions.Where(i => i.GameId == id);
                foreach (var action in actions)
                {
                    db.UCGameplayActions.Remove(action);
                }

                var pstats = db.UCGamePlayerStats.Where(i => i.GameId == id);
                foreach (var ps in pstats)
                {
                    db.UCGamePlayerStats.Remove(ps);
                }

                var tstats = db.UCGameTeamStats.Where(i => i.GameId == id);
                foreach (var ts in tstats)
                {
                    db.UCGameTeamStats.Remove(ts);
                }

                var game = db.UCGames.Find(id);
                if (game != null)
                {
                    db.UCGames.Remove(game);
                }
            }
        }

        public static string ActionToString(UCGameplayAction action, TeamGameplayModel teamModel)
        {
            UCPlayer player = action.PlayerId > 0 ? teamModel.Players.SingleOrDefault(i=>i.Id == action.PlayerId) : null;

            return string.Format("Q{5} <strong>{0}</strong> {1} ({2})... {3}-{4}",
                action.GameTime.ToString("mm:ss"), 
                GameAction.GetAction(action.ActionCode).Text, 
                player == null ? "" : string.Format("{0} {1}. {2}", player.PlayerNumber, player.FirstName.Substring(0,1), player.LastName),
                action.TeamScore1, 
                action.TeamScore2,
                action.Quarter
            );
        }

        public static int UpdateGame(int gameId, int quarter, bool updateScores, UCGameTeamStat ts0, UCGameTeamStat ts1, bool updateTime, DateTime timeLeft, DateTime timerLastModified, GameplayModel gp = null)
        {
            var gameplay = gp ?? GameplayModel.Create(gameId);
            var game = gameplay.Game;
            var now = DateTime.UtcNow;
            bool hasGameUpdates = false;

            if (updateScores)
            {
                for (int i = 0; i < 2; i++)
                {
                    var t = i == 0 ? ts0 : ts1;
                    var ts = gameplay.TeamModels[i].Stat;

                    ts.Q1 = t.Q1;
                    ts.Q2 = t.Q2;
                    ts.Q3 = t.Q3;
                    ts.Q4 = t.Q4;
                    ts.LastUpdate = DateTime.UtcNow;
                }
            }

            if (updateTime)
            {
                game.TimeLeft = timeLeft; //GameHelper.ComputeTimeRemaining(game);
                game.TimeLastModified = timerLastModified;
                hasGameUpdates = true;
            }

            if (game.CurrentQuarter != quarter)
            {
                // Create player stats stats
                game.CurrentQuarter = quarter;
                var q = quarter > 4 ? 4 : quarter;

                for (int i = 0; i < 2; i++)
                {
                    var teamModel = gameplay.TeamModels[i];
                    var stats = teamModel.QuarterPlayerStats;

                    var players = teamModel.Players;
                    foreach (var player in players)
                    {
                        var stat = stats.SingleOrDefault(item => item.PlayerId == player.Id);
                        if (stat == null)
                        {
                            stat = new UCGamePlayerStat();
                            stat.PlayerId = player.Id;
                            stat.TeamId = player.TeamId;
                            stat.GameId = gameplay.GameId;
                            stat.Quarter = q;
                            gameplay.Db.UCGamePlayerStats.Add(stat);
                        }
                    }
                }

                hasGameUpdates = true;
            }

            if (hasGameUpdates)
            {
                game.LastUpdate = now;
            }

            if (hasGameUpdates || updateScores)
            {
                game.LastActivityDate = now;
                gameplay.Db.SaveChanges();
            }

            return 0;
        }
    }
}
