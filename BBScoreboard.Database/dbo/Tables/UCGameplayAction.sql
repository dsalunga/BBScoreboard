CREATE TABLE [dbo].[UCGameplayAction] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [GameId]      INT           NOT NULL,
    [Quarter]     INT           NOT NULL,
    [TeamId]      INT           NOT NULL,
    [PlayerId]    INT           DEFAULT ((-1)) NOT NULL,
    [ActionCode]  INT           NOT NULL,
    [Arg]         INT           NOT NULL,
    [RecPlayerId] INT           DEFAULT ((-1)) NOT NULL,
    [ActionDate]  DATETIME2 (7) DEFAULT (getdate()) NOT NULL,
    [GameTime]    DATETIME2 (7) NOT NULL,
    [TeamScore1]  INT           DEFAULT ((0)) NOT NULL,
    [TeamScore2]  INT           DEFAULT ((0)) NOT NULL,
    [Status]      INT           DEFAULT ((1)) NOT NULL,
    [LastUpdate]  DATETIME2 (7) DEFAULT ('2000-01-01') NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);





