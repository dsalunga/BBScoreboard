CREATE TABLE [dbo].[UCPlayer] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]    NVARCHAR (500) NOT NULL,
    [LastName]     NVARCHAR (500) NOT NULL,
    [PlayerNumber] INT            NOT NULL,
    [PositionId]   INT            NOT NULL,
    [TeamId]       INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

