ALTER TABLE [dbo].[UCPlayer]
    ADD [Active] BIT DEFAULT 1 NOT NULL;
GO

ALTER TABLE [dbo].[UCTeam]
    ADD [Active] BIT DEFAULT 1 NOT NULL;
GO

ALTER TABLE [dbo].[UCGame]
    ADD [IsEnded] BIT DEFAULT 0 NOT NULL;
GO


CREATE TABLE [dbo].[AppConfig] (
    [Id]    INT            IDENTITY (1, 1) NOT NULL,
    [Key]   NVARCHAR (500) NOT NULL,
    [Value] NVARCHAR (500) NOT NULL
);
GO