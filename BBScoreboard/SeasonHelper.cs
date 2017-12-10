using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBScoreboard
{
    public class SeasonHelper
    {
        public static void Delete(int id, BBScoreboardEntities db = null)
        {
            if (id > 0)
            {
                db = db ?? new BBScoreboardEntities();

                var games = db.UCGames.Where(i => i.SeasonId == id);
                foreach (var game in games)
                {
                    GameHelper.Delete(game.Id, db);
                }

                var item = db.UCSeasons.Find(id);
                if (item != null)
                {
                    db.UCSeasons.Remove(item);
                }
            }
        }
    }
}
