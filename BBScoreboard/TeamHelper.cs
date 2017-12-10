using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBScoreboard
{
    public class TeamHelper
    {
        public static int GetTotalPoints(UCGameTeamStat stat)
        {
            return stat.Q1 + stat.Q2 + stat.Q3 + stat.Q4;
        }

        public static void Delete(int id, BBScoreboardEntities db = null)
        {
            if (id > 0)
            {
                db = db ?? new BBScoreboardEntities();

                var tstats = db.UCGameTeamStats.Where(i => i.TeamId == id);
                foreach (var ts in tstats)
                {
                    db.UCGameTeamStats.Remove(ts);
                }

                var players = db.UCPlayers.Where(i=>i.TeamId== id);
                foreach(var player in players)
                {
                    PlayerHelper.Delete(player.Id, db);
                }

                var team = db.UCTeams.Find(id);
                if (team != null)
                {
                    db.UCTeams.Remove(team);
                }
            }
        }
    }
}
