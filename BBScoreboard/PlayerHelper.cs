using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBScoreboard
{
    public class PlayerHelper
    {
        public static int GetPoints(UCGamePlayerStat stat)
        {
            return (stat.FGM - stat.FGM3) * 2 + stat.FGM3 * 3 + stat.FTM;
        }

        public static int GetRebounds(UCGamePlayerStat stat)
        {
            return stat.REBDEF + stat.REBOFF;
        }

        public static int GetFouls(UCGamePlayerStat stat)
        {
            return stat.FOULPER + stat.FOULTECH;
        }

        public static bool IsEmptyStat(UCGamePlayerStat ps)
        {
            return (ps.ASSIST +
                ps.BLOCK +
                ps.FGA +
                ps.FGA3 +
                ps.FGM +
                ps.FGM3 +
                ps.FOULPER +
                ps.FOULTECH +
                ps.FTA +
                ps.FTM +
                ps.REBDEF +
                ps.REBOFF +
                ps.STEAL +
                ps.TURNOVER) == 0;
        }

        public static void Delete(int id, BBScoreboardEntities db = null)
        {
            if (id > 0)
            {
                db = db ?? new BBScoreboardEntities();

                var actions = db.UCGameplayActions.Where(i => i.PlayerId == id);
                foreach (var action in actions)
                {
                    db.UCGameplayActions.Remove(action);
                }

                var pstats = db.UCGamePlayerStats.Where(i => i.PlayerId == id);
                foreach (var ps in pstats)
                {
                    db.UCGamePlayerStats.Remove(ps);
                }

                var player = db.UCPlayers.Find(id);
                if (player != null)
                {
                    db.UCPlayers.Remove(player);
                }
            }
        }

        public static void UpdateStat(UCGamePlayerStat stat, GameActions action, bool apply = true)
        {
            var statPoint = apply ? 1 : -1;
            switch (action)
            {
                case GameActions.FreeThrowAttempt:
                    stat.FTA += statPoint;
                    break;
                case GameActions.FieldGoalAttempt:
                    stat.FGA += statPoint;
                    break;
                case GameActions.FieldGoal3Attempt:
                    stat.FGA3 += statPoint;
                    stat.FGA += statPoint;
                    break;

                case GameActions.FreeThrowMade:
                    stat.FTM += statPoint;
                    stat.FTA += statPoint;
                    break;
                case GameActions.FieldGoalMade:
                    stat.FGM += statPoint;
                    stat.FGA += statPoint;
                    break;
                case GameActions.FieldGoal3Made:
                    stat.FGM3 += statPoint;
                    stat.FGM += statPoint;
                    stat.FGA3 += statPoint;
                    stat.FGA += statPoint;
                    break;

                case GameActions.Assist:
                    stat.ASSIST += statPoint;
                    break;
                case GameActions.Steal:
                    stat.STEAL += statPoint;
                    break;
                case GameActions.Block:
                    stat.BLOCK += statPoint;
                    break;

                case GameActions.ReboundOffensive:
                    stat.REBOFF += statPoint;
                    break;
                case GameActions.RebourdDefensive:
                    stat.REBDEF += statPoint;
                    break;

                case GameActions.Turnover:
                    stat.TURNOVER += statPoint;
                    break;

                case GameActions.FoulPersonal:
                    stat.FOULPER += statPoint;
                    break;
                case GameActions.FoulTechnical:
                    stat.FOULTECH += statPoint;
                    break;
            }
        }
    }
}
