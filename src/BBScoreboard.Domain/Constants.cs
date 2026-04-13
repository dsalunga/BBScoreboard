using System.Collections.Frozen;
using BBScoreboard.Domain.Enums;

namespace BBScoreboard.Domain;

public class GameAction
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public static GameAction GetAction(int id)
    {
        return Actions.GetValueOrDefault(id, new GameAction());
    }

    public static readonly FrozenDictionary<int, GameAction> Actions = new Dictionary<int, GameAction>
    {
        { -1, new GameAction { Id = -1, Text = "Free Throw Missed", Code = "FTm" } },
        { -2, new GameAction { Id = -2, Text = "2-Pointer Missed", Code = "FGm" } },
        { -3, new GameAction { Id = -3, Text = "3-Pointer Missed", Code = "FG3m" } },
        { 1, new GameAction { Id = 1, Text = "Free Throw", Code = "FT" } },
        { 2, new GameAction { Id = 2, Text = "2-Pointer Made", Code = "FG" } },
        { 3, new GameAction { Id = 3, Text = "3-Pointer Made", Code = "FG3" } },
        { 4, new GameAction { Id = 4, Text = "Assist", Code = "AST" } },
        { 5, new GameAction { Id = 5, Text = "Steal", Code = "STL" } },
        { 6, new GameAction { Id = 6, Text = "Block", Code = "BLK" } },
        { 7, new GameAction { Id = 7, Text = "Offensive Rebound", Code = "RBO" } },
        { 8, new GameAction { Id = 8, Text = "Defensive Rebound", Code = "RBD" } },
        { 9, new GameAction { Id = 9, Text = "Turnover", Code = "TO" } },
        { 10, new GameAction { Id = 10, Text = "Personal Foul", Code = "PF" } },
        { 11, new GameAction { Id = 11, Text = "Technical Foul", Code = "TF" } },
    }.ToFrozenDictionary();
}

public static class AccessTypeMap
{
    public static readonly FrozenDictionary<int, string> Types = new Dictionary<int, string>
    {
        { 0, "Read-Only" },
        { 1, "Administrator" },
        { 2, "Statistician" },
    }.ToFrozenDictionary();

    public static string GetAccessType(int access)
    {
        return Types.GetValueOrDefault(access, string.Empty);
    }
}

public static class UCConstants
{
    public static readonly DateTime BaseDate = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
