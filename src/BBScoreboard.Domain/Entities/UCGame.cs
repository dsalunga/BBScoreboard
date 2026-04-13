namespace BBScoreboard.Domain.Entities;

public class UCGame
{
    public int Id { get; set; }
    public int GameNumber { get; set; }
    public int Team1 { get; set; }
    public int Team2 { get; set; }
    public DateTime GameDate { get; set; }
    public int CurrentQuarter { get; set; }
    public int SeasonId { get; set; }
    public string Venue { get; set; } = string.Empty;
    public bool IsStarted { get; set; }
    public DateTime TimeLastModified { get; set; }
    public bool IsTimeOn { get; set; }
    public DateTime TimeLeft { get; set; }
    public DateTime LastActivityDate { get; set; }
    public DateTime LastUpdate { get; set; }
    public DateTime LastUpdateForRefresh { get; set; }
    public bool IsEnded { get; set; }
}
