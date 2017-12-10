using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBScoreboard
{
    public enum PlayerStarter
    {
        Floor = 1,
        Bench = 2
    }

    public class TeamGameplayModel
    {
        public int TeamId { get; set; }
        public GameplayModel Gameplay { get; set; }

        public BBScoreboardEntities Db { get { return Gameplay.Db; } }

        public UCGameTeamStat Stat
        {
            get { return Db.UCGameTeamStats.SingleOrDefault(i => i.TeamId == TeamId && i.GameId == Gameplay.GameId); }
        }

        public UCTeam Team
        {
            get { return Db.UCTeams.Find(TeamId); }
        }

        public IEnumerable<UCGamePlayerStat> PlayerStats
        {
            get { return Db.UCGamePlayerStats.Where(i => i.GameId == Gameplay.GameId && i.TeamId == Team.Id); }
        }

        public IEnumerable<UCGamePlayerStat> QuarterPlayerStats
        {
            get
            {
                var game = Gameplay.Game;
                var q = game.CurrentQuarter > 4 ? 4 : game.CurrentQuarter;
                return Db.UCGamePlayerStats.Where(i => i.GameId == Gameplay.GameId && i.TeamId == Team.Id && i.Quarter == q);
            }
        }

        public IEnumerable<UCGamePlayerStat> OverallPlayerStats
        {
            get
            {
                var game = Gameplay.Game;
                var q = game.CurrentQuarter > 4 ? 4 : game.CurrentQuarter;
                var playerStats = Db.UCGamePlayerStats.Where(i => i.GameId == Gameplay.GameId && i.TeamId == Team.Id);
                var overallStats = playerStats.Where(s => s.Quarter == q);

                foreach (var stat in playerStats)
                {
                    if (stat.Quarter != q)
                    {
                        var playerStat = overallStats.FirstOrDefault(s => s.PlayerId == stat.PlayerId);
                        if (playerStats != null)
                        {
                            playerStat.FGA += stat.FGA;
                            playerStat.FGM += stat.FGM;
                            playerStat.FGA3 += stat.FGA3;
                            playerStat.FGM3 += stat.FGM3;
                            playerStat.FTA += stat.FTA;
                            playerStat.FTM += stat.FTM;
                            playerStat.REBOFF += stat.REBOFF;
                            playerStat.REBDEF += stat.REBDEF;
                            playerStat.ASSIST += stat.ASSIST;
                            playerStat.STEAL += stat.STEAL;
                            playerStat.BLOCK += stat.BLOCK;
                            playerStat.TURNOVER += stat.TURNOVER;
                            playerStat.FOULPER += stat.FOULPER;
                            playerStat.FOULTECH += stat.FOULTECH;
                        }
                    }
                }

                return overallStats;
            }
        }

        public List<PlayerGameplayModel> PlayerModels { get; set; }

        public IEnumerable<UCPlayer> Players
        {
            get { return Db.UCPlayers.Where(i => i.TeamId == Team.Id); }
        }

        public IEnumerable<UCGameplayAction> Actions
        {
            get { return Db.UCGameplayActions.Where(i => i.GameId == Gameplay.GameId && i.TeamId == TeamId); }
        }

        public IEnumerable<UCGameplayAction> QuarterActions
        {
            get
            {
                var game = Gameplay.Game;
                var q = game.CurrentQuarter > 4 ? 4 : game.CurrentQuarter;
                return Db.UCGameplayActions.Where(i => i.GameId == Gameplay.GameId && i.TeamId == TeamId && (q == 4 ? i.Quarter >= q : i.Quarter == q));
            }
        }

        public IEnumerable<UCGameplayAction> GetActions(int quarter, DateTime lastUpdate)
        {
            return Db.UCGameplayActions.Where(i => i.GameId == Gameplay.GameId && i.TeamId == TeamId && (quarter == 4 ? i.Quarter >= quarter : quarter == 4 ? true : i.Quarter == quarter) && (i.ActionDate > lastUpdate || i.LastUpdate > lastUpdate));
        }

        public TeamGameplayModel(GameplayModel gameplay, UCTeam team)
        {
            Gameplay = gameplay;
            TeamId = team.Id;
        }

        public IEnumerable<UCPlayer> GetPlayers(PlayerStarter position)
        {
            return from p in PlayerModels
                   where p.Stat.InFloor == (position == PlayerStarter.Floor)
                   select p.Player;
        }

        private void SetupPlayerStats()
        {
            var game = Gameplay.Game;
            var q = game.CurrentQuarter >= 4 ? 4 : game.CurrentQuarter;

            bool hasChanges = false;
            var stats = QuarterPlayerStats;
            var players = Players;
            if (players.Count() != stats.Count())
            {
                foreach (var player in players)
                {
                    var stat = stats.SingleOrDefault(i => i.PlayerId == player.Id);
                    if (stat == null)
                    {
                        stat = new UCGamePlayerStat();
                        stat.PlayerId = player.Id;
                        stat.TeamId = player.TeamId;
                        stat.GameId = Gameplay.GameId;
                        stat.Quarter = q;
                        Db.UCGamePlayerStats.Add(stat);

                        hasChanges = true;
                    }
                }
            }

            if (hasChanges)
            {
                Db.SaveChanges();
                stats = QuarterPlayerStats;
            }

            // Setup player stats model
            PlayerModels = new List<PlayerGameplayModel>();
            foreach (var player in players)
            {
                PlayerModels.Add(new PlayerGameplayModel(this, player));
            }
        }

        private void SetupTeamStat()
        {
            var item = Db.UCGameTeamStats.SingleOrDefault(i => i.TeamId == TeamId && i.GameId == Gameplay.GameId);
            if (item == null)
            {
                item = new UCGameTeamStat();
                item.GameId = Gameplay.GameId;
                item.TeamId = TeamId;

                Db.UCGameTeamStats.Add(item);
                Db.SaveChanges();
            }
        }

        public void Initialize()
        {
            SetupTeamStat();
            SetupPlayerStats();
        }

        public static TeamGameplayModel Create(GameplayModel gameplay, UCTeam team)
        {
            var model = new TeamGameplayModel(gameplay, team);
            model.Initialize();
            return model;
        }
    }
}