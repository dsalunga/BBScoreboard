using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBScoreboard
{
    public class GameplayModel
    {
        public int GameId { get; set; }
        public BBScoreboardEntities Db { get; set; }

        private UCGame _game = null;
        private UCSeason _season = null;
        public UCGame Game
        {
            get
            {
                if (_game == null)
                {
                    _game = Db.UCGames.Find(GameId);
                }

                return _game;
            }
        }

        public UCSeason Season
        {
            get
            {
                if (_season == null)
                {
                    var game = Game;
                    _season = Db.UCSeasons.Find(game.SeasonId);
                }

                return _season;
            }
        }

        public List<TeamGameplayModel> TeamModels { get; set; }

        public GameplayModel(BBScoreboardEntities db, int gameId)
        {
            Db = db;
            GameId = gameId;
            TeamModels = new List<TeamGameplayModel>();
        }

        public string GetQuarterString()
        {
            var game = Game;
            if (game.CurrentQuarter > 4)
                return "OT" + (game.CurrentQuarter - 4);
            else
                return "Q" + game.CurrentQuarter;
        }

        public void Initialize()
        {
            TeamGameplayModel model = null;
            var team1 = Db.UCTeams.SingleOrDefault(i => i.Id == Game.Team1);
            var team2 = Db.UCTeams.SingleOrDefault(i => i.Id == Game.Team2);

            foreach (var team in new UCTeam[] { team1, team2 })
            {
                if ((model = TeamModels.Find(i => i.Team.Id == team.Id)) == null)
                {
                    model = TeamGameplayModel.Create(this, team);
                    TeamModels.Add(model);
                }
                else
                {
                    model.Initialize();
                }
            }

            //if ((model = TeamModels.Find(i => i.Team.Id == team2.Id)) == null)
            //{
            //    model = TeamGameplayModel.Create(this, team2);
            //    TeamModels.Add(model);
            //}
            //else
            //{
            //    model.Initialize();
            //}
        }

        public void Start(IEnumerable<int> start1, IEnumerable<int> start2, int mins, bool autoStart)
        {
            Action<TeamGameplayModel, IEnumerable<int>> SetStarting5 = (tm, ids) =>
            {
                if (ids.Count() == 5)
                {
                    var stats = tm.PlayerStats;
                    foreach (var id in ids)
                    {
                        var stat = stats.FirstOrDefault(i => i.PlayerId == id);
                        stat.InFloor = true;
                    }
                }
            };

            var team1 = TeamModels[0];
            var team2 = TeamModels[1];

            SetStarting5(team1, start1);
            SetStarting5(team2, start2);

            var game = Game;
            game.CurrentQuarter = 1;
            game.IsStarted = true;
            game.IsTimeOn = autoStart;
            game.TimeLastModified = autoStart ? DateTime.UtcNow : new DateTime(UCConstants.BaseDate.Ticks);
            game.LastActivityDate = DateTime.UtcNow;
            game.TimeLeft = new DateTime(2000, 1, 1, 0, mins, 0, DateTimeKind.Utc);

            Db.SaveChanges();
        }

        public void ResetGame()
        {
            var actions = Db.UCGameplayActions.Where(i => i.GameId == Game.Id);
            foreach (var action in actions)
            {
                Db.UCGameplayActions.Remove(action);
            }

            foreach (var tm in TeamModels)
            {
                foreach (var ps in tm.PlayerStats)
                {
                    Db.UCGamePlayerStats.Remove(ps);
                }

                var stat = tm.Stat;
                if (stat != null)
                {
                    Db.UCGameTeamStats.Remove(tm.Stat);
                }
            }

            Game.IsStarted = false;
            Game.IsTimeOn = false;
            Game.LastActivityDate = DateTime.UtcNow;

            Db.SaveChanges();
        }

        public static GameplayModel Create(int gameId, BBScoreboardEntities db = null)
        {
            var item = new GameplayModel(db == null ? new BBScoreboardEntities() : db, gameId);
            item.Initialize();

            return item;
        }
    }
}
