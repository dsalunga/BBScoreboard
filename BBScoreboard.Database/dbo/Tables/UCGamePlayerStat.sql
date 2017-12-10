CREATE TABLE [dbo].[UCGamePlayerStat] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [PlayerId]   INT           NOT NULL,
    [FGA]        INT           NOT NULL,
    [FGM]        INT           NOT NULL,
    [GameId]     INT           NOT NULL,
    [Quarter]    INT           NOT NULL,
    [FGA3]       INT           NOT NULL,
    [FGM3]       INT           NOT NULL,
    [FTA]        INT           NOT NULL,
    [FTM]        INT           NOT NULL,
    [REBOFF]     INT           NOT NULL,
    [REBDEF]     INT           NOT NULL,
    [ASSIST]     INT           NOT NULL,
    [STEAL]      INT           NOT NULL,
    [BLOCK]      INT           NOT NULL,
    [TURNOVER]   INT           NOT NULL,
    [FOULPER]    INT           NOT NULL,
    [FOULTECH]   INT           NOT NULL,
    [InFloor]    BIT           DEFAULT ((0)) NOT NULL,
    [TeamId]     INT           NOT NULL,
    [LastUpdate] DATETIME2 (7) DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);



