using BBScoreboard.Application.Services;
using BBScoreboard.Domain;
using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Tests;

public class GameHelperTests
{
    [Fact]
    public void GetScore_ReturnsQuarterSum()
    {
        var stat = new UCGameTeamStat { Q1 = 22, Q2 = 18, Q3 = 25, Q4 = 20 };

        Assert.Equal(85, GameHelper.GetScore(stat));
    }

    [Fact]
    public void GetScore_AllZeros_ReturnsZero()
    {
        Assert.Equal(0, GameHelper.GetScore(new UCGameTeamStat()));
    }

    [Theory]
    [InlineData(1, 5, 0, 0, 0)]
    [InlineData(2, 0, 5, 0, 0)]
    [InlineData(3, 0, 0, 5, 0)]
    [InlineData(4, 0, 0, 0, 5)]
    [InlineData(5, 0, 0, 0, 5)] // OT goes to Q4
    public void UpdateScore_UpdatesCorrectQuarter(int quarter, int q1, int q2, int q3, int q4)
    {
        var stat = new UCGameTeamStat();
        GameHelper.UpdateScore(stat, quarter, 5);

        Assert.Equal(q1, stat.Q1);
        Assert.Equal(q2, stat.Q2);
        Assert.Equal(q3, stat.Q3);
        Assert.Equal(q4, stat.Q4);
    }

    [Fact]
    public void UpdateScore_ZeroDelta_NoChange()
    {
        var stat = new UCGameTeamStat { Q1 = 10 };
        GameHelper.UpdateScore(stat, 1, 0);

        Assert.Equal(10, stat.Q1);
    }

    [Fact]
    public void UpdateScore_NegativeDelta_Subtracts()
    {
        var stat = new UCGameTeamStat { Q2 = 15 };
        GameHelper.UpdateScore(stat, 2, -3);

        Assert.Equal(12, stat.Q2);
    }

    [Fact]
    public void GetTeamName_FindTeam_ReturnsName()
    {
        var teams = new List<UCTeam>
        {
            new() { Id = 1, Name = "Eagles" },
            new() { Id = 2, Name = "Hawks" },
        };

        Assert.Equal("Eagles", GameHelper.GetTeamName(teams, 1));
    }

    [Fact]
    public void GetTeamName_NotFound_ReturnsEmpty()
    {
        var teams = new List<UCTeam>();
        Assert.Equal("", GameHelper.GetTeamName(teams, 99));
    }

    [Fact]
    public void GetTeamName_ZeroId_ReturnsEmpty()
    {
        var teams = new List<UCTeam> { new() { Id = 1, Name = "Eagles" } };
        Assert.Equal("", GameHelper.GetTeamName(teams, 0));
    }

    [Fact]
    public void GetTeamName_TwoTeams_FormatsWithSeparator()
    {
        var teams = new List<UCTeam>
        {
            new() { Id = 1, Name = "Eagles" },
            new() { Id = 2, Name = "Hawks" },
        };

        Assert.Equal("Eagles vs Hawks", GameHelper.GetTeamName(teams, 1, 2));
        Assert.Equal("Eagles at Hawks", GameHelper.GetTeamName(teams, 1, 2, "at"));
    }

    [Theory]
    [InlineData(1, "Q1")]
    [InlineData(2, "Q2")]
    [InlineData(3, "Q3")]
    [InlineData(4, "Q4")]
    [InlineData(5, "OT1")]
    [InlineData(6, "OT2")]
    public void GetQuarterString_ReturnsCorrectFormat(int quarter, string expected)
    {
        Assert.Equal(expected, GameHelper.GetQuarterString(quarter));
    }

    [Fact]
    public void ComputeTimeRemaining_TimerOff_ReturnsTimeLeft()
    {
        var game = new UCGame
        {
            IsTimeOn = false,
            TimeLeft = UCConstants.BaseDate.AddMinutes(5),
        };

        Assert.Equal(game.TimeLeft, GameHelper.ComputeTimeRemaining(game));
    }

    [Fact]
    public void ComputeTimeRemaining_TimerOn_DeductsElapsed()
    {
        var game = new UCGame
        {
            IsTimeOn = true,
            TimeLeft = UCConstants.BaseDate.AddMinutes(10),
            TimeLastModified = DateTime.UtcNow.AddSeconds(-30),
        };

        var remaining = GameHelper.ComputeTimeRemaining(game);
        // Should be approximately 9:30 remaining (10 min - 30 sec)
        var expectedMinutes = (remaining - UCConstants.BaseDate).TotalMinutes;
        Assert.InRange(expectedMinutes, 9.4, 9.6);
    }

    [Fact]
    public void ComputeTimeRemaining_TimerOn_DoesNotGoBelowBaseDate()
    {
        var game = new UCGame
        {
            IsTimeOn = true,
            TimeLeft = UCConstants.BaseDate.AddSeconds(1),
            TimeLastModified = DateTime.UtcNow.AddMinutes(-5), // Way past
        };

        var remaining = GameHelper.ComputeTimeRemaining(game);
        Assert.Equal(UCConstants.BaseDate, remaining);
    }

    [Fact]
    public void ActionToString_FormatsCorrectly()
    {
        var action = new UCGameplayAction
        {
            PlayerId = 1,
            ActionCode = 2, // 2-Pointer Made
            GameTime = UCConstants.BaseDate.AddMinutes(8).AddSeconds(30),
            TeamScore1 = 45,
            TeamScore2 = 42,
            Quarter = 2,
        };

        var players = new List<UCPlayer>
        {
            new() { Id = 1, PlayerNumber = 23, FirstName = "Michael", LastName = "Jordan" },
        };

        var result = GameHelper.ActionToString(action, players);
        Assert.Contains("08:30", result);
        Assert.Contains("2-Pointer Made", result);
        Assert.Contains("23 M. Jordan", result);
        Assert.Contains("45-42", result);
        Assert.Contains("Q2", result);
    }
}
