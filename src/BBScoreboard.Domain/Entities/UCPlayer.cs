namespace BBScoreboard.Domain.Entities;

public class UCPlayer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int PlayerNumber { get; set; }
    public int PositionId { get; set; }
    public int TeamId { get; set; }
    public bool Active { get; set; } = true;
}
