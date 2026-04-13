namespace BBScoreboard.Domain.Enums;

public enum GameActions
{
    FreeThrowAttempt = -1,
    FieldGoalAttempt = -2,
    FieldGoal3Attempt = -3,

    FreeThrowMade = 1,
    FieldGoalMade = 2,
    FieldGoal3Made = 3,

    Assist = 4,
    Steal = 5,
    Block = 6,

    ReboundOffensive = 7,
    ReboundDefensive = 8,

    Turnover = 9,

    FoulPersonal = 10,
    FoulTechnical = 11
}
