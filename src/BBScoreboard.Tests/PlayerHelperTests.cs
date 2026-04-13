using BBScoreboard.Application.Services;
using BBScoreboard.Domain.Entities;
using BBScoreboard.Domain.Enums;

namespace BBScoreboard.Tests;

public class PlayerHelperTests
{
    private static UCGamePlayerStat CreateStat() => new();

    [Fact]
    public void GetPoints_TwoPointers_ReturnsCorrectValue()
    {
        var stat = CreateStat();
        stat.FGM = 5;
        stat.FGM3 = 0;
        stat.FTM = 0;

        Assert.Equal(10, PlayerHelper.GetPoints(stat));
    }

    [Fact]
    public void GetPoints_ThreePointers_ReturnsCorrectValue()
    {
        var stat = CreateStat();
        stat.FGM = 3; // FGM includes FGM3
        stat.FGM3 = 3;
        stat.FTM = 0;

        // (3-3)*2 + 3*3 + 0 = 9
        Assert.Equal(9, PlayerHelper.GetPoints(stat));
    }

    [Fact]
    public void GetPoints_MixedScoring_ReturnsCorrectValue()
    {
        var stat = CreateStat();
        stat.FGM = 8;   // 8 total FG made (5 two-pointers + 3 three-pointers)
        stat.FGM3 = 3;  // 3 of those are 3-pointers
        stat.FTM = 4;   // 4 free throws

        // (8-3)*2 + 3*3 + 4 = 10 + 9 + 4 = 23
        Assert.Equal(23, PlayerHelper.GetPoints(stat));
    }

    [Fact]
    public void GetPoints_ZeroStats_ReturnsZero()
    {
        Assert.Equal(0, PlayerHelper.GetPoints(CreateStat()));
    }

    [Fact]
    public void GetRebounds_ReturnsSumOfOffAndDef()
    {
        var stat = CreateStat();
        stat.REBOFF = 3;
        stat.REBDEF = 7;

        Assert.Equal(10, PlayerHelper.GetRebounds(stat));
    }

    [Fact]
    public void GetFouls_ReturnsSumOfPersonalAndTechnical()
    {
        var stat = CreateStat();
        stat.FOULPER = 4;
        stat.FOULTECH = 1;

        Assert.Equal(5, PlayerHelper.GetFouls(stat));
    }

    [Fact]
    public void IsEmptyStat_AllZeros_ReturnsTrue()
    {
        Assert.True(PlayerHelper.IsEmptyStat(CreateStat()));
    }

    [Fact]
    public void IsEmptyStat_HasStats_ReturnsFalse()
    {
        var stat = CreateStat();
        stat.FGA = 1;

        Assert.False(PlayerHelper.IsEmptyStat(stat));
    }

    [Theory]
    [InlineData(GameActions.FreeThrowAttempt, nameof(UCGamePlayerStat.FTA), 1)]
    [InlineData(GameActions.FieldGoalAttempt, nameof(UCGamePlayerStat.FGA), 1)]
    [InlineData(GameActions.FreeThrowMade, nameof(UCGamePlayerStat.FTM), 1)]
    [InlineData(GameActions.FreeThrowMade, nameof(UCGamePlayerStat.FTA), 1)]
    [InlineData(GameActions.FieldGoalMade, nameof(UCGamePlayerStat.FGM), 1)]
    [InlineData(GameActions.FieldGoalMade, nameof(UCGamePlayerStat.FGA), 1)]
    [InlineData(GameActions.Assist, nameof(UCGamePlayerStat.ASSIST), 1)]
    [InlineData(GameActions.Steal, nameof(UCGamePlayerStat.STEAL), 1)]
    [InlineData(GameActions.Block, nameof(UCGamePlayerStat.BLOCK), 1)]
    [InlineData(GameActions.ReboundOffensive, nameof(UCGamePlayerStat.REBOFF), 1)]
    [InlineData(GameActions.ReboundDefensive, nameof(UCGamePlayerStat.REBDEF), 1)]
    [InlineData(GameActions.Turnover, nameof(UCGamePlayerStat.TURNOVER), 1)]
    [InlineData(GameActions.FoulPersonal, nameof(UCGamePlayerStat.FOULPER), 1)]
    [InlineData(GameActions.FoulTechnical, nameof(UCGamePlayerStat.FOULTECH), 1)]
    public void UpdateStat_Apply_IncrementsCorrectField(GameActions action, string fieldName, int expected)
    {
        var stat = CreateStat();
        PlayerHelper.UpdateStat(stat, action, apply: true);

        var actual = (int)typeof(UCGamePlayerStat).GetProperty(fieldName)!.GetValue(stat)!;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void UpdateStat_Unapply_DecrementsField()
    {
        var stat = CreateStat();
        stat.FTM = 5;
        stat.FTA = 5;

        PlayerHelper.UpdateStat(stat, GameActions.FreeThrowMade, apply: false);

        Assert.Equal(4, stat.FTM);
        Assert.Equal(4, stat.FTA);
    }

    [Fact]
    public void UpdateStat_FieldGoal3Made_IncrementsFourFields()
    {
        var stat = CreateStat();
        PlayerHelper.UpdateStat(stat, GameActions.FieldGoal3Made);

        Assert.Equal(1, stat.FGM3);
        Assert.Equal(1, stat.FGM);
        Assert.Equal(1, stat.FGA3);
        Assert.Equal(1, stat.FGA);
    }

    [Fact]
    public void UpdateStat_FieldGoal3Attempt_IncrementsTwoFields()
    {
        var stat = CreateStat();
        PlayerHelper.UpdateStat(stat, GameActions.FieldGoal3Attempt);

        Assert.Equal(1, stat.FGA3);
        Assert.Equal(1, stat.FGA);
    }
}
