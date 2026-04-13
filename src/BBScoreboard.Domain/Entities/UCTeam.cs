namespace BBScoreboard.Domain.Entities;

public class UCTeam
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TeamColor { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
}
