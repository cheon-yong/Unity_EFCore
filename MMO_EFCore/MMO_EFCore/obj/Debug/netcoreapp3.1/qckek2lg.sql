IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Guild] (
    [GuildId] int NOT NULL IDENTITY,
    [GuildName] nvarchar(max) NULL,
    CONSTRAINT [PK_Guild] PRIMARY KEY ([GuildId])
);
GO

CREATE TABLE [Player] (
    [PlayerId] int NOT NULL IDENTITY,
    [Name] nvarchar(20) NOT NULL,
    [GuildId] int NULL,
    CONSTRAINT [PK_Player] PRIMARY KEY ([PlayerId]),
    CONSTRAINT [FK_Player_Guild_GuildId] FOREIGN KEY ([GuildId]) REFERENCES [Guild] ([GuildId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Items] (
    [ItemId] int NOT NULL IDENTITY,
    [SoftDeleted] bit NOT NULL,
    [TemplateId] int NOT NULL,
    [CreateDate] datetime2 NOT NULL DEFAULT (GETDATE()),
    [OwnerId] int NOT NULL,
    CONSTRAINT [PK_Items] PRIMARY KEY ([ItemId]),
    CONSTRAINT [FK_Items_Player_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Player] ([PlayerId]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_Items_OwnerId] ON [Items] ([OwnerId]);
GO

CREATE UNIQUE INDEX [Index_Person_name] ON [Player] ([Name]);
GO

CREATE INDEX [IX_Player_GuildId] ON [Player] ([GuildId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211125151538_HelloMigration', N'5.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Items] ADD [ItemGrade] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211125152023_ItemGrade', N'5.0.10');
GO

COMMIT;
GO

