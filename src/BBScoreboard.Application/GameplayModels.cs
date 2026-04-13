using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Application;

public class GameplayModel
{
    public int GameId { get; set; }
    public UCGame Game { get; set; } = null!;
    public UCSeason? Season { get; set; }
    public List<TeamGameplayModel> TeamModels { get; set; } = [];
}

public class TeamGameplayModel
{
    public int TeamId { get; set; }
    public UCTeam Team { get; set; } = null!;
    public UCGameTeamStat Stat { get; set; } = null!;
    public List<UCGamePlayerStat> PlayerStats { get; set; } = [];
    public List<UCGamePlayerStat> QuarterPlayerStats { get; set; } = [];
    public List<PlayerGameplayModel> PlayerModels { get; set; } = [];
    public List<UCGameplayAction> Actions { get; set; } = [];
    public List<UCPlayer> Players { get; set; } = [];
}

public class PlayerGameplayModel
{
    public int PlayerId { get; set; }
    public UCPlayer Player { get; set; } = null!;
    public UCGamePlayerStat Stat { get; set; } = null!;
    public BasketballPosition? Position { get; set; }
}
