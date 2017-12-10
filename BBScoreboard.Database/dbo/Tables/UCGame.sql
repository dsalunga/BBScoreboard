CREATE TABLE [dbo].[UCGame] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [GameNumber]       INT             NOT NULL,
    [Team1]            INT             NOT NULL,
    [Team2]            INT             NOT NULL,
    [GameDate]         DATETIME2 (7)   NOT NULL,
    [CurrentQuarter]   INT             NOT NULL,
    [SeasonId]         INT             NOT NULL,
    [Venue]            NVARCHAR (2000) NOT NULL,
    [IsStarted]        BIT             DEFAULT ((0)) NOT NULL,
    [TimeLastModified] DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    [IsTimeOn]         BIT             DEFAULT ((0)) NOT NULL,
    [TimeLeft]         DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    [LastActivityDate] DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    [LastUpdate]       DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
	[LastUpdateForRefresh]       DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);



