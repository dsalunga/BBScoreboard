namespace BBScoreboard.Domain.Entities;

public class UCGameplayAction
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int Quarter { get; set; }
    public int TeamId { get; set; }
    public int PlayerId { get; set; } = -1;
    public int ActionCode { get; set; }
    public int Arg { get; set; }
    public int RecPlayerId { get; set; } = -1;
    public DateTime ActionDate { get; set; }
    public DateTime GameTime { get; set; }
    public int TeamScore1 { get; set; }
    public int TeamScore2 { get; set; }
    public int Status { get; set; }
    public DateTime LastUpdate { get; set; }
}
