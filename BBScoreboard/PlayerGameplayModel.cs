using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBScoreboard
{
    public class PlayerGameplayModel
    {
        public int PlayerId { get; set; }
        public TeamGameplayModel TeamModel { get; set; }

        public UCPlayer Player
        {
            get { return TeamModel.Db.UCPlayers.Find(PlayerId); }
        }

        public UCGamePlayerStat Stat
        {
            get
            {
                var game = TeamModel.Gameplay.Game;
                var q = game.CurrentQuarter > 4 ? 4 : game.CurrentQuarter;
                var stat = TeamModel.Db.UCGamePlayerStats.SingleOrDefault(i => i.PlayerId == PlayerId && i.GameId == game.Id && i.Quarter == q);
                return stat;
            }
        }

        public UCGamePlayerStat Stats
        {
            get
            {
                var game = TeamModel.Gameplay.Game;
                var stat = TeamModel.Db.UCGamePlayerStats.SingleOrDefault(i => i.PlayerId == PlayerId && i.GameId == game.Id);
                return stat;
            }
        }

        public BasketballPosition Position
        {
            get { return TeamModel.Db.BasketballPositions.Find(Player.PositionId); }
        }

        public PlayerGameplayModel(TeamGameplayModel teamModel, UCPlayer player)
        {
            TeamModel = teamModel;
            PlayerId = player.Id;
        }
    }
}
