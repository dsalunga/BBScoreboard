CREATE TABLE [BasketballPosition] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Name]      NVARCHAR (500) NOT NULL,
    [ShortName] NVARCHAR (50)  NOT NULL
);
GO


CREATE TABLE [UCGame] (
    [Id]               INT             IDENTITY (1, 1) PRIMARY KEY,
    [GameNumber]       INT             NOT NULL,
    [Team1]            INT             NOT NULL,
    [Team2]            INT             NOT NULL,
    [GameDate]         DATETIME   NOT NULL,
    [CurrentQuarter]   INT             NOT NULL,
    [SeasonId]         INT             NOT NULL,
    [Venue]            NVARCHAR (2000) NOT NULL,
    [IsStarted]        BIT             DEFAULT ((0)) NOT NULL,
    [TimeLastModified] DATETIME        DEFAULT (getdate()) NOT NULL,
    [IsTimeOn]         BIT             DEFAULT ((0)) NOT NULL,
    [TimeLeft]         DATETIME        DEFAULT (getdate()) NOT NULL,
    [LastActivityDate] DATETIME        DEFAULT (getdate()) NOT NULL,
    [LastUpdate]       DATETIME        DEFAULT (getdate()) NOT NULL,
	[LastUpdateForRefresh] DATETIME    DEFAULT (getdate()) NOT NULL
);
GO


CREATE TABLE [UCGameplayAction] (
    [Id]          INT           IDENTITY (1, 1) PRIMARY KEY,
    [GameId]      INT           NOT NULL,
    [Quarter]     INT           NOT NULL,
    [TeamId]      INT           NOT NULL,
    [PlayerId]    INT           DEFAULT ((-1)) NOT NULL,
    [ActionCode]  INT           NOT NULL,
    [Arg]         INT           NOT NULL,
    [RecPlayerId] INT           DEFAULT ((-1)) NOT NULL,
    [ActionDate]  DATETIME DEFAULT (getdate()) NOT NULL,
    [GameTime]    DATETIME NOT NULL,
    [TeamScore1]  INT           DEFAULT ((0)) NOT NULL,
    [TeamScore2]  INT           DEFAULT ((0)) NOT NULL
);
GO


CREATE TABLE [UCGamePlayerStat] (
    [Id]         INT           IDENTITY (1, 1) PRIMARY KEY,
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
    [LastUpdate] DATETIME 	   DEFAULT (getdate()) NOT NULL
);
GO


CREATE TABLE [UCGameTeamStat] (
    [Id]         INT           IDENTITY (1, 1) PRIMARY KEY,
    [TeamId]     INT           NOT NULL,
    [GameId]     INT           NOT NULL,
    [Q1]         INT           NOT NULL,
    [Q2]         INT           NOT NULL,
    [Q3]         INT           NOT NULL,
    [Q4]         INT           NOT NULL,
    [LastUpdate] DATETIME      DEFAULT (getdate()) NOT NULL
);
GO



CREATE TABLE [UCPlayer] (
    [Id]           INT            IDENTITY (1, 1) PRIMARY KEY,
    [FirstName]    NVARCHAR (500) NOT NULL,
    [LastName]     NVARCHAR (500) NOT NULL,
    [PlayerNumber] INT            NOT NULL,
    [PositionId]   INT            NOT NULL,
    [TeamId]       INT            NOT NULL
);
GO


CREATE TABLE [UCSeason] (
    [Id]   INT             IDENTITY (1, 1) PRIMARY KEY,
    [Name] NVARCHAR (2000) NOT NULL
);
GO


CREATE TABLE [UCTeam] (
    [Id]        INT             IDENTITY (1, 1) PRIMARY KEY,
    [Name]      NVARCHAR (2000) NOT NULL,
    [TeamColor] NVARCHAR (50)   NOT NULL
);
GO



-- INSERT DATA --

SET IDENTITY_INSERT [BasketballPosition] ON;
GO

INSERT INTO [BasketballPosition] ([Id], [Name], [ShortName]) VALUES (1, N'Point Guard (PG)', N'PG');
GO
INSERT INTO [BasketballPosition] ([Id], [Name], [ShortName]) VALUES (2, N'Shooting Guard (SG)', N'SG');
GO
INSERT INTO [BasketballPosition] ([Id], [Name], [ShortName]) VALUES (3, N'Small Forward (SF)', N'SF');
GO
INSERT INTO [BasketballPosition] ([Id], [Name], [ShortName]) VALUES (4, N'Power Forward (PF)', N'PF');
GO
INSERT INTO [BasketballPosition] ([Id], [Name], [ShortName]) VALUES (5, N'Center (C)', N'C');
GO

SET IDENTITY_INSERT [BasketballPosition] OFF;
GO