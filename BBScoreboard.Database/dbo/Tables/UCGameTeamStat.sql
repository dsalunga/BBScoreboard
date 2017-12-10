CREATE TABLE [dbo].[UCGameTeamStat] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [TeamId]     INT           NOT NULL,
    [GameId]     INT           NOT NULL,
    [Q1]         INT           NOT NULL,
    [Q2]         INT           NOT NULL,
    [Q3]         INT           NOT NULL,
    [Q4]         INT           NOT NULL,
    [LastUpdate] DATETIME2 (7) DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);



