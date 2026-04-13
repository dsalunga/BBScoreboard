using BBScoreboard.Domain;
using BBScoreboard.Domain.Entities;
using BBScoreboard.Domain.Enums;

namespace BBScoreboard.Tests;

public class DomainModelTests
{
    [Theory]
    [InlineData(-1, "Free Throw Missed", "FTm")]
    [InlineData(-2, "2-Pointer Missed", "FGm")]
    [InlineData(-3, "3-Pointer Missed", "FG3m")]
    [InlineData(1, "Free Throw", "FT")]
    [InlineData(2, "2-Pointer Made", "FG")]
    [InlineData(3, "3-Pointer Made", "FG3")]
    [InlineData(4, "Assist", "AST")]
    [InlineData(5, "Steal", "STL")]
    [InlineData(6, "Block", "BLK")]
    [InlineData(7, "Offensive Rebound", "RBO")]
    [InlineData(8, "Defensive Rebound", "RBD")]
    [InlineData(9, "Turnover", "TO")]
    [InlineData(10, "Personal Foul", "PF")]
    [InlineData(11, "Technical Foul", "TF")]
    public void GameAction_GetAction_ReturnsCorrectAction(int id, string expectedText, string expectedCode)
    {
        var action = GameAction.GetAction(id);

        Assert.Equal(id, action.Id);
        Assert.Equal(expectedText, action.Text);
        Assert.Equal(expectedCode, action.Code);
    }

    [Fact]
    public void GameAction_GetAction_UnknownId_ReturnsDefault()
    {
        var action = GameAction.GetAction(999);

        Assert.Equal(0, action.Id);
        Assert.Empty(action.Text);
        Assert.Empty(action.Code);
    }

    [Fact]
    public void GameAction_Actions_Contains14Entries()
    {
        Assert.Equal(14, GameAction.Actions.Count);
    }

    [Theory]
    [InlineData(0, "Read-Only")]
    [InlineData(1, "Administrator")]
    [InlineData(2, "Statistician")]
    public void AccessTypeMap_ReturnsCorrectType(int access, string expected)
    {
        Assert.Equal(expected, AccessTypeMap.GetAccessType(access));
    }

    [Fact]
    public void AccessTypeMap_UnknownAccess_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, AccessTypeMap.GetAccessType(99));
    }

    [Fact]
    public void UCConstants_BaseDate_IsYear2000Utc()
    {
        Assert.Equal(2000, UCConstants.BaseDate.Year);
        Assert.Equal(1, UCConstants.BaseDate.Month);
        Assert.Equal(1, UCConstants.BaseDate.Day);
        Assert.Equal(DateTimeKind.Utc, UCConstants.BaseDate.Kind);
    }

    [Fact]
    public void GameActions_EnumValues_MatchActionDictionary()
    {
        // Verify each enum value has a corresponding GameAction entry
        foreach (GameActions ga in Enum.GetValues<GameActions>())
        {
            var action = GameAction.GetAction((int)ga);
            Assert.Equal((int)ga, action.Id);
            Assert.NotEmpty(action.Text);
            Assert.NotEmpty(action.Code);
        }
    }
}
