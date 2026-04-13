namespace BBScoreboard.Domain.Entities;

public class UCGameTeamStat
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int GameId { get; set; }
    public int Q1 { get; set; }
    public int Q2 { get; set; }
    public int Q3 { get; set; }
    public int Q4 { get; set; }
    public DateTime LastUpdate { get; set; }
}
