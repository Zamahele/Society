BEGIN TRANSACTION;
GO

CREATE TABLE [society].[ErrorLogs] (
    [Id] int NOT NULL IDENTITY,
    [RequestId] nvarchar(120) NOT NULL,
    [Path] nvarchar(500) NOT NULL,
    [Method] nvarchar(20) NOT NULL,
    [ErrorMessage] nvarchar(4000) NOT NULL,
    [StackTrace] nvarchar(max) NULL,
    [UserId] nvarchar(450) NULL,
    [OccurredAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_ErrorLogs] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260424125422_AddErrorLogsTable', N'8.0.0');
GO

COMMIT;
GO

