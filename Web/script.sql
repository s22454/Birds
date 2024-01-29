IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240129145123_Initial')
BEGIN
    CREATE TABLE [Models] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(255) NULL,
        [Data] varbinary(max) NULL,
        CONSTRAINT [PK_Models] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240129145123_Initial')
BEGIN
    CREATE TABLE [Predictions] (
        [Id] uniqueidentifier NOT NULL,
        [BirdName] nvarchar(255) NULL,
        [TimeSpent] datetime2 NOT NULL,
        [ModelId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Predictions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Predictions_Models_ModelId] FOREIGN KEY ([ModelId]) REFERENCES [Models] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240129145123_Initial')
BEGIN
    CREATE TABLE [Photos] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(255) NULL,
        [Data] varbinary(max) NULL,
        [PredictionId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Photos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Photos_Predictions_PredictionId] FOREIGN KEY ([PredictionId]) REFERENCES [Predictions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240129145123_Initial')
BEGIN
    CREATE UNIQUE INDEX [IX_Photos_PredictionId] ON [Photos] ([PredictionId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240129145123_Initial')
BEGIN
    CREATE INDEX [IX_Predictions_ModelId] ON [Predictions] ([ModelId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240129145123_Initial')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240129145123_Initial', N'7.0.15');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240129203753_Init')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240129203753_Init', N'7.0.15');
END;
GO

